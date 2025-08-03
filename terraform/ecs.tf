resource "aws_ecr_repository" "app" {
  name = "${var.project_name}-app-repo"
}

resource "aws_cloudwatch_log_group" "ecs_logs" {
  name = "/ecs/ecs-logs"
}

resource "aws_cloudwatch_log_group" "sidecar" {
  name = "/ecs/gts-sidecar-logs"
}

resource "aws_ecs_cluster" "main" {
  name = "${var.project_name}-cluster"
}

data "aws_caller_identity" "current" {}

# --- Web API Task & Service ---

resource "aws_ecs_task_definition" "app_service" {
  family             = "${var.project_name}-web-api"
  network_mode       = "awsvpc"
  requires_compatibilities = ["FARGATE"]
  cpu                = "256"
  memory             = "512"
  execution_role_arn = aws_iam_role.ecs_task_execution_role.arn
  task_role_arn      = aws_iam_role.app_task_role.arn

  container_definitions = jsonencode([
    {
      name  = "web-api"
      image = "${data.aws_caller_identity.current.account_id}.dkr.ecr.${var.aws_region}.amazonaws.com/${var.ecr_repo_name}:latest"
      portMappings = [
        {
          containerPort = var.app_container_port
          hostPort      = var.app_container_port
        }
      ]
      environment = [
        { name = "CORS_ORIGIN", value = "https://peerspace.online" },
        { name = "ASPNETCORE_ENVIRONMENT", value = "Production" },
        { name = "DYNAMODB_TABLE_NAME", value = aws_dynamodb_table.main.name },
        { name = "SQS_QUEUE_URL", value = aws_sqs_queue.inbound_queue.id },
        { name = "REDIS_ENDPOINT", value = aws_elasticache_cluster.redis.cache_nodes[0].address },
        {
          name = "GoogleAuthSettings__RedirectUri"
          value = "https://www.peerspace.online/api/users/google-callback"
        },
        {
          "name" : "JwtSettings__Issuer",
          "value" : "social-media9-api-dev"
        },
        {
          "name" : "JwtSettings__Audience",
          "value" : "social-media9-users-dev"
        },
        {
          "name" : "DynamoDbSettings__Region",
          "value" : var.aws_region
        },
        {
          "name" : "DynamoDbSettings__UsersTableName",
          "value" : "${var.project_name}-main-table"
        },
        {
          "name" : "DynamoDbSettings__FollowsTableName",
          "value" : "${var.project_name}-main-table"
        }
      ]
      secrets = [
        {
          name      = "GTS_HOOK_SECRET"
          valueFrom = aws_secretsmanager_secret.gts_hook_secret.arn
        },
        {
          "name" : "GoogleAuthSettings__ClientId",
          valueFrom = "${data.aws_secretsmanager_secret.google_secrets.arn}:google_client_id::"
        },
        {
          "name" : "GoogleAuthSettings__ClientSecret",
          valueFrom = "${data.aws_secretsmanager_secret.google_secrets.arn}:google_client_secret::"
        },
        {
          "name" : "JwtSettings__Secret",
          valueFrom = "${data.aws_secretsmanager_secret.jwt_secrets.arn}:jwt_secret::"
        },

      ]
      logConfiguration = {
        logDriver = "awslogs"
        options = {
          awslogs-group         = aws_cloudwatch_log_group.ecs_logs.name
          awslogs-create-group  = "true"
          awslogs-region        = var.aws_region
          awslogs-stream-prefix = "ecs"
        }
      }
    }
  ])
}

resource "aws_ecs_service" "app_service" {
  name            = "${var.project_name}-app-service"
  cluster         = aws_ecs_cluster.main.id
  task_definition = aws_ecs_task_definition.app_service.arn
  desired_count   = 0
  launch_type     = "FARGATE"

  network_configuration {
    subnets = module.vpc.private_subnets
    security_groups = [aws_security_group.ecs_sg.id]
  }

  load_balancer {
    target_group_arn = aws_lb_target_group.web_api.arn
    container_name   = "web-api"
    container_port   = var.app_container_port
  }

  depends_on = [aws_lb_listener.main_listener]
}

# # --- Worker Service Task & Service ---
#
# resource "aws_ecs_task_definition" "worker" {
#   family                   = "${var.project_name}-worker"
#   network_mode             = "awsvpc"
#   requires_compatibilities = ["FARGATE"]
#   cpu                      = "256"
#   memory                   = "512"
#   execution_role_arn       = aws_iam_role.ecs_task_execution_role.arn
#   task_role_arn            = aws_iam_role.app_task_role.arn
#
#   container_definitions = jsonencode([{
#     name  = "worker"
#     image = var.ecr_image_url_worker
#     environment = [
#       { name = "ASPNETCORE_ENVIRONMENT", value = "Production" },
#       { name = "DYNAMODB_TABLE_NAME", value = aws_dynamodb_table.main.name },
#       { name = "SQS_QUEUE_URL", value = aws_sqs_queue.main.id },
#       { name = "REDIS_ENDPOINT", value = aws_elasticache_cluster.redis.cache_nodes[0].address }
#     ]
#     logConfiguration = {
#       logDriver = "awslogs"
#       options = {
#         "awslogs-group"         = "/ecs/${var.project_name}/worker"
#         "awslogs-region"        = var.aws_region
#         "awslogs-stream-prefix" = "ecs"
#       }
#     }
#   }])
# }
#
# resource "aws_ecs_service" "worker" {
#   name            = "${var.project_name}-worker-service"
#   cluster         = aws_ecs_cluster.main.id
#   task_definition = aws_ecs_task_definition.worker.arn
#   desired_count   = 1
#   launch_type     = "FARGATE"
#
#   network_configuration {
#     subnets         = module.vpc.private_subnets
#     security_groups = [aws_security_group.ecs_sg.id]
#   }
# }
#

resource "aws_ecs_task_definition" "gts_sidecar" {
  family        = "${var.project_name}-gts-sidecar"
  network_mode  = "awsvpc"
  requires_compatibilities = ["FARGATE"]
  cpu           = "256"
  memory        = "512"
  execution_role_arn = aws_iam_role.ecs_task_execution_role.arn
  # This task role needs SQS SendMessage permissions
  task_role_arn = aws_iam_role.gts_task_role.arn

  container_definitions = jsonencode([
    {
      name = "gotosocial"
      # Use the official GTS image
      image = "superseriousbusiness/gotosocial:latest"
      portMappings = [{ containerPort = 8080 }]

      # Configure GTS via environment variables, which reference secrets
      environment = [
        { name = "GTS_HOST", value = "federation.peerspace.online" },
        { name = "GTS_DOMAIN", value = "peerspace.online" },
        { name = "GTS_USER_DOMAIN", value = "peerspace.online" },
        { name = "GTS_PORT", value = "8080" },
        { name = "GTS_DB_TYPE", value = "sqlite" },
        { name = "GTS_DB_ADDRESS", value = "/tmp/gts.db" },

        { name = "GTS_ACCOUNT_PROVIDER_HOOK_ENABLED", value = "true" },
        {
          name  = "GTS_ACCOUNT_PROVIDER_HOOK_ENDPOINT",
          value = "http://${aws_lb.main.dns_name}:${var.internal_api_port}/internal/v1/user"
        },

        { name = "GTS_INBOX_DELIVERY_HOOK_ENABLED", value = "true" },
        { name = "GTS_INBOX_DELIVERY_HOOK_TYPE", value = "sqs" },
        { name = "GTS_INBOX_DELIVERY_HOOK_SQS_QUEUE_URL", value = aws_sqs_queue.main.id },
        { name = "AWS_REGION", value = var.aws_region }
      ]
      secrets = [
        {
          name      = "GTS_ACCOUNT_PROVIDER_HOOK_SECRET"
          valueFrom = aws_secretsmanager_secret.gts_hook_secret.arn
        }
      ]

      logConfiguration = {
        logDriver = "awslogs"
        options = {
          awslogs-group         = aws_cloudwatch_log_group.sidecar.name
          awslogs-create-group  = "true"
          awslogs-region        = var.aws_region
          awslogs-stream-prefix = "ecs"
        }
      }
    }
  ])
}

resource "aws_ecs_service" "gts_sidecar" {
  name            = "${var.project_name}-gts-sidecar-service"
  cluster         = aws_ecs_cluster.main.id
  task_definition = aws_ecs_task_definition.gts_sidecar.arn
  desired_count   = 1
  launch_type     = "FARGATE"

  network_configuration {
    subnets = module.vpc.private_subnets
    security_groups = [aws_security_group.ecs_sg.id]
  }

  load_balancer {
    target_group_arn = aws_lb_target_group.gts_sidecar.arn
    container_name   = "gotosocial"
    container_port   = 8080
  }
}

resource "aws_secretsmanager_secret" "gts_config" {
  name = "${var.project_name}/gts-config"
}


resource "aws_secretsmanager_secret_version" "gts_config_version" {
  secret_id = aws_secretsmanager_secret.gts_config.id

  secret_string = <<-EOT

    domain: "peerspace.online"
    listen-address: "0.0.0.0:8080"
    database-type: "sqlite"
    database-path: "/data/gts.db"

    disable-registration: true
    disable-login: true
    disable-settings: true


    account-provider-hook:
      enabled: true
      endpoint: "http://${aws_lb.main.dns_name}:${var.internal_api_port}/internal/v1/user"

      secret: "${random_string.hook_secret.result}"

    inbox-delivery-hook:
      enabled: true
      type: "sqs"
      sqs-queue-url: "${aws_sqs_queue.inbound_queue.id}"
      sqs-region: "${var.aws_region}"
  EOT
}

resource "random_string" "hook_secret" {
  length  = 32
  special = false
}

resource "aws_secretsmanager_secret" "gts_hook_secret" {
  name = "${var.project_name}/gts-hook-secret"
}

resource "aws_secretsmanager_secret_version" "gts_hook_secret_version" {
  secret_id     = aws_secretsmanager_secret.gts_hook_secret.id
  secret_string = random_string.hook_secret.result
}