# v2rayN Multi-Port Feature Fork

This is a fork of [v2rayN](https://github.com/2dust/v2rayN) with additional multi-port functionality.

## Feature Description

This fork adds the ability to assign different local proxy ports to different nodes/profiles, allowing you to:
- Run multiple proxy instances simultaneously
- Route different applications through different nodes
- Test multiple nodes at the same time

## Workflow Overview

### Automatic Upstream Monitoring
- **Check Upstream Release** (`check-upstream-release.yml`): Runs daily to detect new upstream releases
- When a new release is detected, an issue is automatically created

### Syncing with Upstream
- **Sync Upstream** (`sync-upstream.yml`): Manually triggered to merge upstream changes
- Creates a PR for review before merging

### Building & Releasing
- **Release Multi-Port Version** (`release-multiport.yml`): Builds all platforms when a release is published
- Supported platforms:
  - Windows x64
  - Windows arm64
  - Linux x64
  - Linux arm64
  - macOS x64
  - macOS arm64

## Usage

### Syncing with New Upstream Release

1. When you receive an issue notification about a new upstream release
2. Go to Actions → "Sync Upstream" → Run workflow
3. Enter the upstream tag (optional, defaults to latest)
4. Review and merge the PR
5. Create a new release with tag like `7.17.0-multiport`

### Building a New Release

1. Go to Releases → Create new release
2. Tag format: `X.Y.Z-multiport` (e.g., `7.17.0-multiport`)
3. The build workflow will automatically trigger
4. Download artifacts from the release page

## Implementation Notes

### Key Files for Multi-Port Feature

The multi-port feature should be implemented in these files:
- `ServiceLib/Models/ConfigItems.cs` - Add per-profile port configuration
- `ServiceLib/Models/ProfileItem.cs` - Add LocalPort property to profiles
- `ServiceLib/Services/CoreConfig/V2ray/V2rayInboundService.cs` - Generate multiple inbounds
- `ServiceLib/Services/CoreConfig/Singbox/SingboxInboundService.cs` - Same for sing-box

### Configuration Structure

```csharp
// Each profile can have its own local port
public class ProfileItem
{
    // ... existing properties
    public int? CustomLocalPort { get; set; }  // If null, use default
}
```

## License

Same as upstream v2rayN - [GPL-3.0](LICENSE)
