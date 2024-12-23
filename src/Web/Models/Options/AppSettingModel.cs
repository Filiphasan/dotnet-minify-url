namespace Web.Models.Options;

public class AppSettingModel
{
    public required AppSettingMongoModel MongoDb { get; set; }
    public required AppSettingRedisModel Redis { get; set; }
    public required AppSettingUrlTokenModel UrlToken { get; set; }
}

public class AppSettingMongoModel
{
    public required string User { get; set; }
    public required string Password { get; set; }
    public required string Host { get; set; }
    public required string Port { get; set; }
    public required string Database { get; set; }

    public string ConnectionString => $"mongodb://{User}:{Password}@{Host}:{Port}";
}

public class AppSettingRedisModel
{
    public required string Host { get; set; }
    public required string Port { get; set; }
    public required string Password { get; set; }
    public required int Database { get; set; }
}

public class AppSettingUrlTokenModel
{
    public required int PoolingSize { get; set; }
    public required int ExpirationDays { get; set; }
    public required string EpochDate { get; set; }
}