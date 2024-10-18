namespace BKey.Email.Serverless.EmailIngestion;
public class SnsNotification
{
    public string? Type {  get; set; }
    public Guid MessageId { get; set; }
    public string? TopicArn { get; set; }
    public string? Subject { get; set; }
    public string? Message { get; set; }
    public DateTime Timestamp { get; set; }
    public int SignatureVersion { get; set; }
    public string? Signature { get; set; }
    public string? SigningCertURL { get; set; }
    public string? UnsubscribeURL { get; set; }
}
