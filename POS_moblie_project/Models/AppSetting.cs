using SQLite;

namespace POS_moblie_project.Models;

[Table("AppSettings")]
public class AppSetting
{
    [PrimaryKey]
    public string Key { get; set; }

    public string Value { get; set; } = string.Empty;

    public AppSetting()
    {
    }

    public AppSetting(string key, string value)
    {
        Key = key;
        Value = value;
    }
}