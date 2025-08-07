output "alb_dns_name" {
  description = "The public DNS name of the Application Load Balancer."
  value       = aws_lb.main_app.dns_name
}

output "ecr_repository_url" {
  description = "The URL of the ECR repository to push Docker images to."
  value       = aws_ecr_repository.app.repository_url
}

output "dynamodb_table_name" {
  description = "The name of the main DynamoDB table."
  value       = aws_dynamodb_table.main.name
}

output "sqs_inbox_queue_url" {
  description = "The URL of the inbox SQS queue."
  value       = aws_sqs_queue.inbound_queue.id
}

output "sqs_oubox_queue_url" {
  description = "The URL of the outbox SQS queue."
  value       = aws_sqs_queue.outbound_queue.id
}

output "dns_records_to_create_manually" {
  description = "MANUAL ACTION REQUIRED: Create these records at your DNS provider."
  value = {
    "frontend_record" = {
      "instructions" = "Create a record for your root domain pointing to the CloudFront distribution."
      "record_name"  = "peerspace.online"
      "record_type"  = "A (Alias)"
      "record_value" = aws_cloudfront_distribution.frontend.domain_name
    }


    "backend_records" = {
      "instructions" = "Create CNAME records for your API and Federation subdomains pointing to the Application Load Balancer."
      "api_record" = {
        "record_name"  = "api.peerspace.online"
        "record_type"  = "CNAME"
        "record_value" = aws_lb.main_app.dns_name
      }
      "federation_record" = {
        "record_name"  = "fed.peerspace.online"
        "record_type"  = "CNAME"
        "record_value" = aws_lb.federation.dns_name
      }

      "media_record" = {
        "record_name"  = "media.peerspace.online"
        "record_type"  = "CNAME"
        "record_value" = aws_cloudfront_distribution.media.domain_name
      }
    }
  }
}