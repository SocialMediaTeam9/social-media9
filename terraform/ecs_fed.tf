# resource "aws_secretsmanager_secret" "gts_config_yaml" {
#   name = "${var.project_name}/gts-config-yaml"
# }
#
# resource "aws_secretsmanager_secret_version" "gts_config_yaml_version" {
#   secret_id     = aws_secretsmanager_secret.gts_config_yaml.id
#   # Using heredoc syntax to embed the YAML configuration.
#   secret_string = <<-EOT
#     # GoToSocial config file provided by Terraform
#     host: "fed.peerspace.online"
#     user-domain: "fed.peerspace.online"
#     port: 8080
#     database-type: "sqlite"
#     database-path: "/tmp/gts.db"
#
#     registration:
#       open: false
#
#     login:
#       enabled: false
#
#     # Account Provider Hook Configuration
#     account-provider-hook:
#       enabled: true
#       endpoint: "http://${aws_lb.main.dns_name}:${var.internal_api_port}/internal/v1/user"
#       # This tells GTS to get the secret from the named environment variable
#       secret_env_var: "GTS_HOOK_SECRET"
#
#     # Inbox Delivery Hook Configuration
#     inbox-delivery-hook:
#       enabled: true
#       type: "sqs"
#       sqs-queue-url: "${aws_sqs_queue.inbound_queue.id}"
#       sqs-region: "${var.aws_region}"
#   EOT
# }

# resource "aws_efs_file_system" "config" {
#   creation_token = "${var.project_name}-gts-config"
# }

# resource "aws_efs_access_point" "gts_config" {
#   file_system_id = aws_efs_file_system.config.id
# }
#
# resource "aws_efs_mount_target" "gts_config" {
#   count           = length(module.vpc.private_subnets)
#   file_system_id  = aws_efs_file_system.config.id
#   subnet_id       = module.vpc.private_subnets[count.index]
#   security_groups = [aws_security_group.ecs_sg.id]
# }