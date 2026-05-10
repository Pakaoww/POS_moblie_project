using SQLite;

namespace POS_moblie_project.Models;

[Table("Categories")]
public class Category
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [NotNull]
    public string Name { get; set; }

    public int SortOrder { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public Category()
    {
    }

    public Category(string name, int sortOrder = 0)
    {
        Name = name;
        SortOrder = sortOrder;
        CreatedAt = DateTime.Now;
    }
}