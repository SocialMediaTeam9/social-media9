resource "aws_secretsmanager_secret" "neo4j_credentials" {
  name = "${var.project_name}/neo4j-credentials"
  description = "Stores the URI, username, and password for the Neo4j AuraDB instance."
}

resource "aws_s3_bucket" "lambda_deployments" {
  bucket = "${var.project_name}-lambda-deployments"

  lifecycle {
    prevent_destroy = true
  }
}

# 2. Enforce private access. No one should be able to read this bucket publicly.
resource "aws_s3_bucket_public_access_block" "lambda_deployments" {
  bucket = aws_s3_bucket.lambda_deployments.id

  block_public_acls       = true
  block_public_policy     = true
  ignore_public_acls      = true
  restrict_public_buckets = true
}

# 3. Enable versioning to keep a history of your deployment packages.
resource "aws_s3_bucket_versioning" "lambda_deployments" {
  bucket = aws_s3_bucket.lambda_deployments.id
  versioning_configuration {
    status = "Enabled"
  }
}
# IMPORTANT: After you run 'terraform apply', you must go to the AWS Secrets Manager console,
# find this secret, and MANUALLY set its value to a JSON object with the keys
# "uri", "username", and "password" from your AuraDB instance.

# --- 2. IAM Role for the Sync Lambda ---
resource "aws_iam_role" "dynamodb_to_neo4j_lambda_role" {
  name = "${var.project_name}-dynamodb-to-neo4j-lambda-role"
  assume_role_policy = jsonencode({
    Version   = "2012-10-17",
    Statement = [{
      Action    = "sts:AssumeRole",
      Effect    = "Allow",
      Principal = { Service = "lambda.amazonaws.com" }
    }]
  })
}

# --- 3. IAM Policy for the Sync Lambda ---
resource "aws_iam_policy" "lambda_permissions" {
  name   = "${var.project_name}-lambda-permissions"
  policy = jsonencode({
    Version   = "2012-10-17",
    Statement = [
      {
        Sid      = "AllowDynamoDBStreamRead",
        Effect   = "Allow",
        Action   = ["dynamodb:GetRecords", "dynamodb:GetShardIterator", "dynamodb:DescribeStream", "dynamodb:ListStreams"],
        Resource = aws_dynamodb_table.main.stream_arn
      },
      {
        Sid      = "AllowCloudWatchLogs",
        Effect   = "Allow",
        Action   = ["logs:CreateLogGroup", "logs:CreateLogStream", "logs:PutLogEvents"],
        Resource = "arn:aws:logs:*:*:*"
      },
      {
        Sid      = "AllowSecretsManagerReadNeo4j",
        Effect   = "Allow",
        Action   = "secretsmanager:GetSecretValue",
        Resource = aws_secretsmanager_secret.neo4j_credentials.arn
      }
    ]
  })
}

resource "aws_iam_role_policy_attachment" "lambda_perms_attach" {
  role       = aws_iam_role.dynamodb_to_neo4j_lambda_role.name
  policy_arn = aws_iam_policy.lambda_permissions.arn
}

resource "aws_iam_role_policy_attachment" "lambda_vpc_attach" {
  role       = aws_iam_role.dynamodb_to_neo4j_lambda_role.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AWSLambdaVPCAccessExecutionRole"
}

# --- 4. Security Group for the Lambda ---
# This SG must allow outbound internet access to reach AuraDB.
resource "aws_security_group" "lambda_sg" {
  name   = "${var.project_name}-lambda-sg"
  vpc_id = module.vpc.vpc_id
  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }
}

# --- 5. The AWS Lambda Function and Trigger ---
resource "aws_lambda_function" "dynamodb_to_neo4j_syncer" {
  s3_bucket        = aws_s3_bucket.lambda_deployments.id
  s3_key           = "dynamodb-neo4j-syncer.zip"
  source_code_hash = filebase64sha256("dummy.zip") # Use a dummy file for initial creation
  function_name    = "${var.project_name}-dynamodb-to-neo4j-syncer"
  role             = aws_iam_role.dynamodb_to_neo4j_lambda_role.arn
  handler          = "DynamoDbNeptuneSyncer::DynamoDbNeptuneSyncer.Function::FunctionHandler"
  runtime          = "dotnet8"
  timeout          = 30

  vpc_config {
    subnet_ids         = module.vpc.private_subnets # Must be in private subnets to use NAT Gateway
    security_group_ids = [aws_security_group.lambda_sg.id]
  }

  environment {
    variables = {
      NEO4J_SECRET_ARN = aws_secretsmanager_secret.neo4j_credentials.arn
    }
  }

  depends_on = [aws_s3_bucket.lambda_deployments]
}

resource "aws_lambda_event_source_mapping" "dynamodb_trigger" {
  event_source_arn  = aws_dynamodb_table.main.stream_arn
  function_name     = aws_lambda_function.dynamodb_to_neo4j_syncer.arn
  starting_position = "LATEST"
}