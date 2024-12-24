namespace Web.Common.Models.Options;

public class AppSettingModel
{
    public required AppSettingServerModel Server { get; set; }
    public required AppSettingMongoModel MongoDb { get; set; }
    public required AppSettingRedisModel Redis { get; set; }
    public required AppSettingUrlTokenModel UrlToken { get; set; }
}

public class AppSettingServerModel
{
    public required string Scheme { get; set; }
    public required string Host { get; set; }
    public required string Port { get; set; }

    public string Url => $"{Scheme}://{Host}:{Port}";
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
    public required int ExtendSize { get; set; }
    public required int ExpirationDays { get; set; }
    public required string EpochDate { get; set; }
}