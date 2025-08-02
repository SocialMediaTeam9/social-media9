resource "aws_iam_role" "ecs_task_execution_role" {
  name = "${var.project_name}-ecs-execution-role"
  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [{
      Action = "sts:AssumeRole"
      Effect = "Allow"
      Principal = {
        Service = "ecs-tasks.amazonaws.com"
      }
    }]
  })
}

resource "aws_iam_policy_attachment" "ecs_task_policy_att" {
  name       = "${var.ecs_cluster_name}-task-exec-policy-attachment"
  policy_arn = aws_iam_policy.ecs_task_exec_policy.arn
  roles = [aws_iam_role.ecs_task_execution_role.name]
}

# resource "aws_iam_role_policy_attachment" "ecs_task_execution_policy_attachment" {
#   role       = aws_iam_role.ecs_task_execution_role.name
#   policy_arn = "arn:aws:iam::aws:policy/service-role/AmazonECSTaskExecutionRolePolicy"
# }

resource "aws_iam_policy" "ecs_task_exec_policy" {
  name = "${var.ecs_cluster_name}-task-exec-policy"
  description = "Policy for ECS task execution role"

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Action = [
          "ecr:GetAuthorizationToken",
          "ecr:BatchCheckLayerAvailability",
          "ecr:GetDownloadUrlForLayer",
          "ecr:BatchGetImage",
          "logs:CreateLogStream",
          "logs:PutLogEvents",
          "logs:CreateLogGroup"
        ],
        Effect = "Allow",
        Resource = "*"
      },
      {
        Effect = "Allow",
        Action = [
          "secretsmanager:GetSecretValue"
        ],
        Resource = "*"
      }
    ]
  })
}

resource "aws_iam_role" "app_task_role" {
  name = "${var.project_name}-app-task-role"
  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [{
      Action = "sts:AssumeRole"
      Effect = "Allow"
      Principal = {
        Service = "ecs-tasks.amazonaws.com"
      }
    }]
  })
}

resource "aws_iam_policy" "app_permissions" {
  name        = "${var.project_name}-app-permissions-policy"
  description = "Permissions for the Nexusphere application to access AWS resources."
  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Action   = ["dynamodb:*"]
        Effect   = "Allow"
        Resource = aws_dynamodb_table.main.arn
      },
      {
        Action   = ["sqs:*"]
        Effect   = "Allow"
        Resource = aws_sqs_queue.main.arn
      },
      {
        Action   = ["elasticache:Describe*", "elasticache:List*"]
        Effect   = "Allow"
        Resource = "*"
      }
    ]
  })
}

resource "aws_iam_role_policy_attachment" "app_permissions_attachment" {
  role       = aws_iam_role.app_task_role.name
  policy_arn = aws_iam_policy.app_permissions.arn
}


resource "aws_security_group" "alb_sg" {
  name        = "${var.project_name}-alb-sg"
  description = "Allow HTTP/HTTPS traffic to the ALB"
  vpc_id      = module.vpc.vpc_id

  ingress {
    from_port   = 80
    to_port     = 80
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  ingress {
    from_port   = 443
    to_port     = 443
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }
}

resource "aws_security_group" "ecs_sg" {
  name        = "${var.project_name}-ecs-sg"
  description = "Allow traffic from the ALB to the ECS tasks"
  vpc_id      = module.vpc.vpc_id


  ingress {
    description     = "Allow public traffic from ALB"
    from_port       = 0
    to_port         = 0
    protocol        = "-1"
    security_groups = [aws_security_group.alb_sg.id]
  }

  ingress {
    from_port       = var.app_container_port
    to_port         = var.app_container_port
    protocol        = "tcp"
    security_groups = [aws_security_group.alb_sg.id]
  }

  ingress {
    description = "Allow internal traffic from other ECS tasks"
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    self        = true
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }
}

resource "aws_security_group" "redis_sg" {
  name        = "${var.project_name}-redis-sg"
  description = "Allow traffic from the ECS tasks to Redis"
  vpc_id      = module.vpc.vpc_id

  ingress {
    from_port       = 6379
    to_port         = 6379
    protocol        = "tcp"
    security_groups = [aws_security_group.ecs_sg.id]
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }
}


resource "aws_iam_role" "gts_task_role" {
  name = "${var.project_name}-gts-task-role"

  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [{
      Action = "sts:AssumeRole"
      Effect = "Allow"
      Principal = {
        Service = "ecs-tasks.amazonaws.com"
      }
    }]
  })
}

resource "aws_iam_policy" "gts_sqs_send_policy" {
  name        = "${var.project_name}-gts-sqs-send-policy"
  description = "Allows the GoToSocial service to send messages to our SQS queue."

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect   = "Allow"
        Action   = "sqs:SendMessage"
        Resource = aws_sqs_queue.main.arn
      }
    ]
  })
}

resource "aws_iam_role_policy_attachment" "gts_task_role_policy_attachment" {
  role       = aws_iam_role.gts_task_role.name
  policy_arn = aws_iam_policy.gts_sqs_send_policy.arn
}