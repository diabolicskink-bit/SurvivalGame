using System.Text.Json;
using System.Text.Json.Serialization;

namespace SurvivalGame.Domain;

public sealed class FirearmDefinitionLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public FirearmCatalog LoadDirectory(string directoryPath)
    {
        if (string.IsNullOrWhiteSpace(directoryPath))
        {
            throw new ArgumentException("Firearm definition directory path cannot be empty.", nameof(directoryPath));
        }

        if (!Directory.Exists(directoryPath))
        {
            throw new DirectoryNotFoundException($"Firearm definition directory does not exist: {directoryPath}");
        }

        var catalog = new FirearmCatalog();

        LoadAmmunitionFileIfPresent(Path.Combine(directoryPath, "ammunition.json"), catalog);
        LoadFeedDeviceFileIfPresent(Path.Combine(directoryPath, "feed_devices.json"), catalog);
        LoadWeaponFileIfPresent(Path.Combine(directoryPath, "weapons.json"), catalog);

        return catalog;
    }

    private static void LoadAmmunitionFileIfPresent(string filePath, FirearmCatalog catalog)
    {
        if (!File.Exists(filePath))
        {
            return;
        }

        var rows = ReadRows<AmmunitionDto>(filePath);
        foreach (var row in rows)
        {
            catalog.AddAmmunition(row.ToDefinition(filePath));
        }
    }

    private static void LoadFeedDeviceFileIfPresent(string filePath, FirearmCatalog catalog)
    {
        if (!File.Exists(filePath))
        {
            return;
        }

        var rows = ReadRows<FeedDeviceDto>(filePath);
        foreach (var row in rows)
        {
            catalog.AddFeedDevice(row.ToDefinition(filePath));
        }
    }

    private static void LoadWeaponFileIfPresent(string filePath, FirearmCatalog catalog)
    {
        if (!File.Exists(filePath))
        {
            return;
        }

        var rows = ReadRows<WeaponDto>(filePath);
        foreach (var row in rows)
        {
            catalog.AddWeapon(row.ToDefinition(filePath));
        }
    }

    private static IReadOnlyList<T> ReadRows<T>(string filePath)
    {
        var json = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<List<T>>(json, JsonOptions)
            ?? throw new InvalidDataException($"Firearm definition file is empty or invalid: {filePath}");
    }

    private sealed class AmmunitionDto
    {
        public string? ItemId { get; set; }

        public string? Name { get; set; }

        public string? Size { get; set; }

        public string? Variant { get; set; }

        public int Damage { get; set; }

        public AmmunitionDefinition ToDefinition(string sourcePath)
        {
            return new AmmunitionDefinition(
                RequiredItemId(ItemId, sourcePath, "ammunition item id"),
                RequiredString(Name, sourcePath, ItemId, "name"),
                new AmmoSizeId(RequiredString(Size, sourcePath, ItemId, "size")),
                RequiredString(Variant, sourcePath, ItemId, "variant"),
                Damage
            );
        }
    }

    private sealed class FeedDeviceDto
    {
        public string? ItemId { get; set; }

        public string? Name { get; set; }

        public FeedDeviceKind Kind { get; set; }

        public string? AmmoSize { get; set; }

        public int Capacity { get; set; }

        public string[]? CompatibleWeaponFamilies { get; set; }

        public FeedDeviceDefinition ToDefinition(string sourcePath)
        {
            return new FeedDeviceDefinition(
                RequiredItemId(ItemId, sourcePath, "feed device item id"),
                RequiredString(Name, sourcePath, ItemId, "name"),
                Kind,
                new AmmoSizeId(RequiredString(AmmoSize, sourcePath, ItemId, "ammo size")),
                Capacity,
                CompatibleWeaponFamilies
            );
        }
    }

    private sealed class WeaponDto
    {
        public string? ItemId { get; set; }

        public string? Name { get; set; }

        public string? WeaponFamily { get; set; }

        public string[]? AcceptedAmmoSizes { get; set; }

        public FeedDeviceKind FeedKind { get; set; }

        public int BuiltInCapacity { get; set; }

        public int EffectiveRangeTiles { get; set; }

        public int MaximumRangeTiles { get; set; }

        public string[]? CompatibleFeedDeviceIds { get; set; }

        public WeaponDefinition ToDefinition(string sourcePath)
        {
            if (AcceptedAmmoSizes is null || AcceptedAmmoSizes.Length == 0)
            {
                throw new InvalidDataException($"Weapon '{ItemId}' in '{sourcePath}' is missing accepted ammo sizes.");
            }

            return new WeaponDefinition(
                RequiredItemId(ItemId, sourcePath, "weapon item id"),
                RequiredString(Name, sourcePath, ItemId, "name"),
                RequiredString(WeaponFamily, sourcePath, ItemId, "weapon family"),
                AcceptedAmmoSizes.Select(size => new AmmoSizeId(size)),
                FeedKind,
                BuiltInCapacity,
                EffectiveRangeTiles,
                MaximumRangeTiles,
                CompatibleFeedDeviceIds?.Select(id => new ItemId(id))
            );
        }
    }

    private static ItemId RequiredItemId(string? value, string sourcePath, string propertyName)
    {
        return new ItemId(RequiredString(value, sourcePath, null, propertyName));
    }

    private static string RequiredString(string? value, string sourcePath, string? itemId, string propertyName)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        var itemText = string.IsNullOrWhiteSpace(itemId) ? "definition" : $"item '{itemId}'";
        throw new InvalidDataException($"{itemText} in '{sourcePath}' is missing {propertyName}.");
    }
}
