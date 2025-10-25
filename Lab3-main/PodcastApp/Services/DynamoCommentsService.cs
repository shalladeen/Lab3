using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace PodcastApp.Services
{
    public class DynamoCommentsService
    {
        private readonly IAmazonDynamoDB _ddb;
        private readonly string _table;

        public DynamoCommentsService(IConfiguration cfg)
        {
            // Default AWS region
            var region = RegionEndpoint.GetBySystemName(cfg["AWS:Region"] ?? "us-east-1");
            _ddb = new AmazonDynamoDBClient(region);

            // DynamoDB table name (must match your AWS console)
            _table = cfg["AWS:DynamoTable"] ?? "Comments";
        }

        // -------------------------------------------------------------
        // ADD COMMENT
        // -------------------------------------------------------------
        public async Task AddAsync(string episodeId, string podcastId, string userId, string text)
        {
            var commentId = Guid.NewGuid().ToString();
            var ts = DateTime.UtcNow.ToString("o");

            var item = new Dictionary<string, AttributeValue>
            {
                ["EpisodeID"] = new AttributeValue { S = episodeId },
                ["CommentID"] = new AttributeValue { S = commentId },
                ["PodcastID"] = new AttributeValue { S = podcastId },
                ["UserID"] = new AttributeValue { S = userId },
                ["Text"] = new AttributeValue { S = text },
                ["Timestamp"] = new AttributeValue { S = ts }
            };

            await _ddb.PutItemAsync(_table, item);
        }

        // -------------------------------------------------------------
        // LIST COMMENTS FOR A SPECIFIC EPISODE
        // -------------------------------------------------------------
        public async Task<List<Dictionary<string, AttributeValue>>> ListForEpisodeAsync(string episodeId)
        {
            var resp = await _ddb.QueryAsync(new QueryRequest
            {
                TableName = _table,
                KeyConditionExpression = "EpisodeID = :ep",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    [":ep"] = new AttributeValue { S = episodeId }
                }
            });

            return resp.Items;
        }

        // -------------------------------------------------------------
        // EDIT COMMENT (only by same user within 24 hours)
        // -------------------------------------------------------------
        public async Task<bool> EditAsync(string episodeId, string commentId, string userId, string newText)
        {
            var get = await _ddb.GetItemAsync(_table, new Dictionary<string, AttributeValue>
            {
                ["EpisodeID"] = new AttributeValue { S = episodeId },
                ["CommentID"] = new AttributeValue { S = commentId }
            });

            if (get.Item.Count == 0)
                return false;

            if (get.Item["UserID"].S != userId)
                return false;

            var ts = DateTime.Parse(get.Item["Timestamp"].S, null, System.Globalization.DateTimeStyles.RoundtripKind);
            if (DateTime.UtcNow - ts > TimeSpan.FromHours(24))
                return false;

            await _ddb.UpdateItemAsync(new UpdateItemRequest
            {
                TableName = _table,
                Key = new()
                {
                    ["EpisodeID"] = new AttributeValue { S = episodeId },
                    ["CommentID"] = new AttributeValue { S = commentId }
                },
                UpdateExpression = "SET #t = :new",
                ExpressionAttributeNames = new() { ["#t"] = "Text" },
                ExpressionAttributeValues = new() { [":new"] = new AttributeValue { S = newText } }
            });

            return true;
        }
    }
}
