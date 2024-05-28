using System.Text.Json.Serialization;

namespace Protocol
{
    class OpenConnectionInput
    {
        [JsonPropertyName("nickname")]
        public required string Nickname { get; set; }
    }

    class OpenConnectionResponse { }


    class RegisterNicknameInput
    {
        [JsonPropertyName("nickname")]
        public required string Password { get; set; }
    }

    class LoginNicknameInput
    {
        [JsonPropertyName("nickname")]
        public required string Password { get; set; }
    }

    class JoinTopicInput
    {
        [JsonPropertyName("topic")]
        public required string Topic { get; set; }
    }

    class LeaveTopicInput
    {
        [JsonPropertyName("topic")]
        public required string Topic { get; set; }
    }

    class SendMessageInput
    {
        [JsonPropertyName("topic")]
        public required string Topic { get; set; }
        [JsonPropertyName("content")]
        public required string Content { get; set; }
    }

    class GenericErrorResponse
    {
        [JsonPropertyName("error")]
        public required string ErrorString { get; set; }
    }

    class Response
    {
        public required int RequestID { get; set; }
        public required bool IsSuccess { get; set; }
        public required string? PayloadSerialized { get; set; }
    }
}