# resource "aws_opensearch_domain" "main" {
#   domain_name    = var.project_name
#   engine_version = "OpenSearch_2.11"
#
#   cluster_config {
#     instance_type  = "t3.small.search"
#     instance_count = 1
#   }
#
#   ebs_options {
#     ebs_enabled = true
#     volume_size = 10
#     volume_type = "gp3"
#   }
#
#   vpc_options {
#     subnet_ids         = module.vpc.private_subnets
#     security_group_ids = [aws_security_group.opensearch_sg.id]
#   }
#
#   advanced_security_options {
#     enabled                        = true
#     internal_user_database_enabled = true
#     master_user_options {
#       master_user_name     = jsondecode(aws_secretsmanager_secret_version.opensearch_master_user_version.secret_string).username
#       master_user_password = jsondecode(aws_secretsmanager_secret_version.opensearch_master_user_version.secret_string).password
#     }
#   }
#
#   encrypt_at_rest { enabled = true }
#
#   domain_endpoint_options {
#     enforce_https       = true
#     tls_security_policy = "Policy-Min-TLS-1-2-2019-07"
#   }
#
#   access_policies = jsonencode({
#     Version = "2012-10-17",
#     Statement = [{
#       Effect = "Allow",
#       Principal = { AWS = "*" },
#       Action = "es:*",
#       Resource = "${aws_opensearch_domain.main.arn}/*"
#     }]
#   })
#
#   timeouts { create = "30m" }
# }