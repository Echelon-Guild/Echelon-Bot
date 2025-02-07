[CmdletBinding()]
Param(
    [Parameter(Mandatory = $true, Position = 0)]
    [string]$StorageAccountName,

    [Parameter(Mandatory = $true, Position = 1)]
    [string]$StorageAccountKey
)

# In your discord bot, you MUST upload the specs to Azure Table Storage for the bot to pull them for use in signup.

# Emotes can be found at https://storeechbotpublic.blob.core.windows.net/echelon-bot-public-images/SpecIcons.zip

# Modify the dictionary below before you run this script to match the IDs provided for each when you upload.

$specs = @{
    "evoker" = @{ "devastation" = "1337108789509226568"; "preservation" = "1337108774715920476"; "augmentation" = "1337108489205583975" }
    "warrior" = @{ "protection" = "1337108054528884826"; "fury" = "1337108039353761956"; "arms" = "1337108016331231372" }
    "warlock" = @{ "destruction" = "1337107995351449691"; "demonology" = "1337107973826412649"; "affliction" = "1337107958005501962" }
    "shaman" = @{ "restoration" = "1337107940825370756"; "enhancement" = "1337107926141370459"; "elemental" = "1337107910844743750" }
    "rogue" = @{ "subtlety" = "1337107878150017024"; "outlaw" = "1337107861796556871"; "assassination" = "1337107845744689212" }
    "priest" = @{ "shadow" = "1337107820872597595"; "holy" = "1337107804250574868"; "discipline" = "1337107783451148308" }
    "paladin" = @{ "retribution" = "1337107752920547460"; "protection" = "1337107734113423432"; "holy" = "1337107719059931258" }
    "monk" = @{ "windwalker" = "1337107635253547140"; "mistweaver" = "1337107606895988779"; "brewmaster" = "1337107581667115159" }
    "mage" = @{ "frost" = "1337107566643253369"; "arcane" = "1337107551229055128"; "fire" = "1337107541234567890" }
    "hunter" = @{ "survival" = "1337107519801393289"; "marksmanship" = "1337107505347825815"; "beast_mastery" = "1337107487668703447" }
    "druid" = @{ "restoration" = "1337107464122007642"; "guardian" = "1337107441900453953"; "feral" = "1337107427270852670"; "balance" = "1337107409302323281" }
    "death_knight" = @{ "unholy" = "1337107392738885733"; "frost" = "1337107326766682154"; "blood" = "1337107310455033990" }
    "demon_hunter" = @{ "vengeance" = "1337107296580276368"; "havoc" = "1337107276137500815" }
}

$tableName = "StoredEmotes"

# Create a Storage Context
$ctx = New-AzStorageContext -StorageAccountName $storageAccountName -StorageAccountKey $storageAccountKey

# Ensure the table exists
$existingTable = Get-AzStorageTable -Name $tableName -Context $ctx -ErrorAction SilentlyContinue
if (-not $existingTable) {
    New-AzStorageTable -Name $tableName -Context $ctx
    Write-Output "Table '$tableName' created."
} else {
    Write-Output "Table '$tableName' already exists."
}

# Get the Table reference
$table = Get-AzStorageTable -Name $tableName -Context $ctx

# Iterate through the dictionary and upload data
foreach ($class in $specs.Keys) {
    foreach ($spec in $specs[$class].Keys) {
        $emoteID = $specs[$class][$spec]

        # Create a table entity
        $entity =  @{
            ClassName = $class
            SpecName = $spec
            EmoteID = $emoteID
        }

        # Insert entity into Azure Table Storage
        Add-AzTableRow -Table $table.CloudTable -PartitionKey $class -RowKey $spec -property $entity
        Write-Output "Uploaded: $class - $spec ($emoteID)"
    }
}

Write-Output "Upload complete!"
