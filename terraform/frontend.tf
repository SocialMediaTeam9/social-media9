data "aws_acm_certificate" "main" {
  domain   = "www.${var.domain_name}"
  statuses = ["ISSUED"]
  provider = aws.us_east_1
  most_recent = true
}

resource "aws_s3_bucket" "frontend" {
  bucket = "peerspace.online"
}

resource "aws_s3_bucket_public_access_block" "frontend" {
  bucket = aws_s3_bucket.frontend.id

  block_public_acls       = true
  block_public_policy     = true
  ignore_public_acls      = true
  restrict_public_buckets = true
}

resource "aws_cloudfront_origin_access_control" "frontend" {
  name                              = "OAC for ${aws_s3_bucket.frontend.bucket}"
  description                       = "Origin Access Control for the Nexusphere frontend"
  origin_access_control_origin_type = "s3"
  signing_behavior                  = "always"
  signing_protocol                  = "sigv4"
}

resource "aws_cloudfront_distribution" "frontend" {
  enabled             = true
  is_ipv6_enabled     = true
  comment             = "CDN for Nexusphere frontend"
  default_root_object = "index.html"

  aliases = [
    var.domain_name,
    "api.${var.domain_name}",
    "federation.${var.domain_name}"
  ]

  origin {
    domain_name              = aws_s3_bucket.frontend.bucket_regional_domain_name
    origin_id                = "S3-${aws_s3_bucket.frontend.bucket}"
    origin_access_control_id = aws_cloudfront_origin_access_control.frontend.id
  }

  origin {
    domain_name = aws_lb.main.dns_name
    origin_id   = "ALB-Peerspace-Backend"
    custom_origin_config {
      http_port  = 443
      https_port = 443
      origin_protocol_policy = "https-only"
      origin_ssl_protocols = ["TLSv1.2"]
    }
  }

  default_cache_behavior {
    allowed_methods = ["GET", "HEAD", "OPTIONS"]
    cached_methods = ["GET", "HEAD"]
    target_origin_id = "S3-${aws_s3_bucket.frontend.bucket}"

    cache_policy_id = "658327ea-f89d-4fab-a63d-7e88639e58f6"
    viewer_protocol_policy = "redirect-to-https"
    min_ttl                = 0
    default_ttl            = 3600
    max_ttl                = 86400
  }

  ordered_cache_behavior {
    path_pattern     = "/api/*"
    allowed_methods = ["GET", "HEAD", "OPTIONS", "PUT", "POST", "PATCH", "DELETE"]
    cached_methods = ["GET", "HEAD"]
    target_origin_id = "ALB-Peerspace-Backend"

    viewer_protocol_policy = "https-only"

    origin_request_policy_id = "216adef6-5c7f-47e4-b989-5492eafa07d3"
    cache_policy_id          = "4135ea2d-6df8-44a3-9df3-4b5a84be39ad"
  }

  ordered_cache_behavior {
    path_pattern           = "/users/*"
    target_origin_id       = "ALB-Peerspace-Backend"
    allowed_methods = ["GET", "HEAD", "OPTIONS", "PUT", "POST", "PATCH", "DELETE"]
    cached_methods = ["GET", "HEAD"]
    viewer_protocol_policy = "https-only"

    origin_request_policy_id = "216adef6-5c7f-47e4-b989-5492eafa07d3"
    cache_policy_id          = "4135ea2d-6df8-44a3-9df3-4b5a84be39ad"
  }
  ordered_cache_behavior {
    path_pattern           = "/.well-known/*"
    target_origin_id       = "ALB-Peerspace-Backend"
    allowed_methods = ["GET", "HEAD", "OPTIONS", "PUT", "POST", "PATCH", "DELETE"]
    cached_methods = ["GET", "HEAD"]
    viewer_protocol_policy = "https-only"

    origin_request_policy_id = "216adef6-5c7f-47e4-b989-5492eafa07d3"
    cache_policy_id          = "4135ea2d-6df8-44a3-9df3-4b5a84be39ad"
  }

  viewer_certificate {
    acm_certificate_arn = data.aws_acm_certificate.main.arn
    ssl_support_method       = "sni-only"
    minimum_protocol_version = "TLSv1.2_2021"
  }

  restrictions {
    geo_restriction {
      restriction_type = "none"
    }
  }
}

resource "aws_s3_bucket_policy" "frontend" {
  bucket = aws_s3_bucket.frontend.id
  policy = jsonencode({
    Version = "2012-10-17"
    Statement = {
      Effect   = "Allow"
      Principal = { Service = "cloudfront.amazonaws.com" }
      Action   = "s3:GetObject"
      Resource = "${aws_s3_bucket.frontend.arn}/*"
      Condition = {
        StringEquals = {
          "AWS:SourceArn" = aws_cloudfront_distribution.frontend.arn
        }
      }
    }
  })
}
