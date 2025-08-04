set -e

echo "Creating social_media9_Users table..."

aws dynamodb create-table \
  --table-name social_media9_Users \
  --attribute-definitions \
    AttributeName=userId,AttributeType=S \
    AttributeName=GoogleId,AttributeType=S \
    AttributeName=Username,AttributeType=S \
  --key-schema AttributeName=userId,KeyType=HASH \
  --global-secondary-indexes '[
    {
      "IndexName": "GoogleId-index",
      "KeySchema": [{"AttributeName": "GoogleId", "KeyType": "HASH"}],
      "Projection": {"ProjectionType": "ALL"},
      "ProvisionedThroughput": {"ReadCapacityUnits": 5, "WriteCapacityUnits": 5}
    },
    {
      "IndexName": "Username-index",
      "KeySchema": [{"AttributeName": "Username", "KeyType": "HASH"}],
      "Projection": {"ProjectionType": "ALL"},
      "ProvisionedThroughput": {"ReadCapacityUnits": 5, "WriteCapacityUnits": 5}
    }
  ]' \
  --provisioned-throughput ReadCapacityUnits=5,WriteCapacityUnits=5 \
  --region us-west-1

echo "Creating social_media9_Follows table..."

aws dynamodb create-table \
  --table-name social_media9_Follows \
  --attribute-definitions \
    AttributeName=followerId,AttributeType=S \
    AttributeName=followeeId,AttributeType=S \
  --key-schema \
    AttributeName=followerId,KeyType=HASH \
    AttributeName=followeeId,KeyType=RANGE \
  --provisioned-throughput ReadCapacityUnits=5,WriteCapacityUnits=5 \
  --region us-east-1

echo "Tables created successfully."
