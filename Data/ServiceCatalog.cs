namespace HomelabDashboard.Data;

public interface IServiceCatalog
{
    IReadOnlyList<ServiceInfo> All { get; }
    ServiceInfo? BySlug(string slug);
}

/// <summary>
/// Hard-coded/mocked service catalog — same "mocked functionality" the
/// original static site shipped with. Swap for a real Docker/Caddy
/// discovery source later; nothing else in the UI needs to change.
/// </summary>
public sealed class ServiceCatalog : IServiceCatalog
{
    public IReadOnlyList<ServiceInfo> All { get; }

    public ServiceCatalog()
    {
        All = BuildCatalog();
    }

    public ServiceInfo? BySlug(string slug) =>
        All.FirstOrDefault(s => string.Equals(s.Slug, slug, StringComparison.OrdinalIgnoreCase));

    private static List<ServiceInfo> BuildCatalog() =>
    [
        new ServiceInfo
        {
            Slug = "pihole",
            Name = "Pi-hole",
            Icon = "🛡️",
            Host = "pihole.lan",
            LaunchUrl = "http://pihole.lan/admin",
            AccentToken = "green",
            Status = ServiceStatus.Running,
            CardDescription = "Network-wide DNS ad blocker. Intercepts DNS queries and blocks advertising, tracking, and malicious domains before they reach any device on the LAN.",
            HeroDescription = "Network-wide DNS-based ad blocker. Blocks ads, trackers, and malicious domains for every device on the LAN.",
            Sections =
            [
                new DetailSection
                {
                    Heading = "What it does",
                    Paragraphs =
                    [
                        "Pi-hole acts as a DNS sinkhole for the entire network. When any device makes a DNS request for an ad-serving or tracking domain, Pi-hole returns a null response instead — the ad simply never loads. Unlike browser extensions, Pi-hole works on every device: smart TVs, phones, IoT devices, game consoles — anything on your network."
                    ]
                },
                new DetailSection
                {
                    Heading = "Key features in use",
                    BulletList =
                    [
                        "<strong>Blocklists</strong> — multiple community-maintained lists covering ads, tracking, malware, and telemetry domains.",
                        "<strong>Local DNS</strong> — resolves all <code>*.lan</code> hostnames to the Pi's IP address so Caddy can route them.",
                        "<strong>DHCP</strong> — optionally serves as DHCP server so all devices automatically get Pi-hole as their DNS.",
                        "<strong>Query log</strong> — real-time view of every DNS query on the network with allow/block controls."
                    ]
                },
                new DetailSection
                {
                    Heading = "Setting Pi-hole as your DNS",
                    Paragraphs =
                    [
                        "The easiest method is to set it at the router level so all devices automatically use it:"
                    ],
                    CodeBlock = "Router DHCP Settings → DNS Server → 192.168.1.x (your Pi's IP)",
                    TipLabel = "Important:",
                    TipBody = "You must use Pi-hole as DNS to resolve <code>*.lan</code> hostnames. Without it, all the Caddy-proxied services will be unreachable by name."
                },
                new DetailSection
                {
                    Heading = "Adding local DNS records",
                    Paragraphs = ["Add one record per service so hostnames resolve to the Pi (where Caddy is listening):"],
                    CodeBlock = "Pi-hole Admin → Settings → Local DNS → DNS Records\n\nDomain              IP\n──────────────────────────────\nthehold.lan         192.168.1.x\npihole.lan          192.168.1.x\nabs.lan             192.168.1.x\nbooks.lan           192.168.1.x\nnotes.lan           192.168.1.x\ncloud.lan           192.168.1.x\nsync.lan            192.168.1.x\nphotos.lan          192.168.1.x\ndraw.lan            192.168.1.x\nspacedeck.lan       192.168.1.x"
                },
                new DetailSection
                {
                    Heading = "Temporarily disabling",
                    Paragraphs =
                    [
                        "If something is being incorrectly blocked, you can pause Pi-hole from the admin dashboard for 5, 30, or 300 seconds without stopping the container. A whitelist entry is usually a better long-term solution."
                    ]
                }
            ]
        },

        new ServiceInfo
        {
            Slug = "caddy",
            Name = "Caddy",
            Icon = "🚦",
            Host = "Reverse Proxy · *.lan",
            LaunchUrl = null,
            AccentToken = "teal",
            Status = ServiceStatus.Running,
            CardDescription = "The harbour master. Routes all *.lan traffic to the correct container with automatic TLS via your local certificate authority. You're looking through it right now.",
            HeroDescription = "The harbour master — automatic HTTPS reverse proxy for all *.lan services.",
            Sections =
            [
                new DetailSection
                {
                    Heading = "What it does",
                    Paragraphs =
                    [
                        "Caddy is the front door to every service in The Hold. It listens on port 80 and 443, routes traffic to the correct Docker container based on the hostname, and handles TLS automatically using its built-in Certificate Authority. Every <code>.lan</code> hostname you access goes through Caddy first.",
                        "Because Caddy manages its own local CA, you only need to trust one certificate (the root CA) and every service automatically gets a signed cert — no more browser warnings."
                    ]
                },
                new DetailSection
                {
                    Heading = "How the routing works",
                    Paragraphs = ["The Caddyfile maps each hostname to a Docker container's internal address. For example:"],
                    CodeBlock = "pihole.lan {\n    reverse_proxy pihole:80\n}\n\nphotos.lan {\n    reverse_proxy immich-server:2283\n}\n\nbooks.lan {\n    reverse_proxy calibre-web:8083\n}",
                },
                new DetailSection
                {
                    Heading = "Local CA & Certificate",
                    Paragraphs =
                    [
                        "Caddy generates a local root CA on first run, stored in <code>/data/caddy/pki/authorities/local/</code>. The certificate you download from the <a href=\"/#cert\">Certificate section</a> is this root CA. Once trusted by a device, all <code>*.lan</code> sites show a green padlock."
                    ],
                    TipLabel = "Tip:",
                    TipBody = "If you regenerate the CA (e.g., after a data wipe), you'll need to re-download and re-trust the certificate on all devices."
                },
                new DetailSection
                {
                    Heading = "DNS requirement",
                    Paragraphs =
                    [
                        "For <code>*.lan</code> hostnames to resolve, Pi-hole must be set as the DNS server on each device (or via your router's DHCP settings), and custom DNS records must be added in Pi-hole pointing each hostname to the Pi's IP address."
                    ],
                    CodeBlock = "# Pi-hole Local DNS Records (Settings → Local DNS → DNS Records)\nthehold.lan        → 192.168.1.x\npihole.lan         → 192.168.1.x\nphotos.lan         → 192.168.1.x\n# ... one entry per service"
                },
                new DetailSection
                {
                    Heading = "Useful paths",
                    CodeBlock = "Config:     /etc/caddy/Caddyfile\nData/Certs: /data/caddy/\nLogs:       docker logs caddy -f"
                }
            ]
        },

        new ServiceInfo
        {
            Slug = "audiobookshelf",
            Name = "Audiobookshelf",
            Icon = "🎧",
            Host = "abs.lan",
            LaunchUrl = "http://abs.lan",
            AccentToken = "gold",
            Status = ServiceStatus.Running,
            CardDescription = "Self-hosted audiobook and podcast server. Tracks progress across devices, syncs bookmarks, and streams directly to mobile apps without any cloud accounts.",
            HeroDescription = "Self-hosted audiobook and podcast server with cross-device progress sync — no Audible, no subscriptions.",
            Sections =
            [
                new DetailSection
                {
                    Heading = "What it does",
                    Paragraphs =
                    [
                        "Audiobookshelf is a full-featured self-hosted server for your audiobook and podcast collection. It streams audio directly from the Pi, tracks listening progress across devices, syncs bookmarks and chapter positions, and supports multiple users — all on your local network (and via Tailscale remotely)."
                    ]
                },
                new DetailSection
                {
                    Heading = "Getting started",
                    NumberedList =
                    [
                        "Open <a href=\"http://abs.lan\" target=\"_blank\">abs.lan</a> in a browser or the mobile app.",
                        "Log in with your credentials (ask Jordan if you haven't got them).",
                        "Browse your library — audiobooks are organized by author and series.",
                        "Tap any book to start streaming; progress is saved automatically."
                    ],
                    TipLabel = "📱 Mobile app:",
                    TipBody = "Install the free <strong>Audiobookshelf</strong> app (iOS/Android). In Settings, set the server URL to <code>http://abs.lan</code> (on home Wi-Fi) or the Tailscale IP when away. Log in and your progress follows you everywhere."
                },
                new DetailSection
                {
                    Heading = "Adding books",
                    Paragraphs =
                    [
                        "Drop audiobook folders into the library path on the Pi. ABS will auto-scan and import them. Supported formats include M4B, MP3, FLAC, OGG, and more. Metadata is fetched automatically from Audnexus/OpenLibrary."
                    ],
                    CodeBlock = "Library path: /mnt/data/audiobooks/\nFormat:       Author Name/Book Title/files.mp3"
                },
                new DetailSection
                {
                    Heading = "Podcasts",
                    Paragraphs =
                    [
                        "ABS can also subscribe to podcast RSS feeds and automatically download new episodes. Great for archiving shows you want to keep without relying on any streaming service."
                    ]
                }
            ]
        },

        new ServiceInfo
        {
            Slug = "calibre",
            Name = "Calibre-Web",
            Icon = "📚",
            Host = "books.lan",
            LaunchUrl = "http://books.lan",
            AccentToken = "purple",
            Status = ServiceStatus.Running,
            CardDescription = "eBook library browser and server backed by your Calibre database. Browse, read, download, and send books directly to your Kindle or reader apps.",
            HeroDescription = "Clean web interface for your Calibre eBook library. Browse, read, download, and send books to your reader.",
            Sections =
            [
                new DetailSection
                {
                    Heading = "What it does",
                    Paragraphs =
                    [
                        "Calibre-Web serves your existing Calibre library through a beautiful browser interface. It supports EPUB reading in-browser, downloading books in multiple formats, and sending directly to a Kindle email address. Think of it as a private Goodreads + lending library, served entirely from your Pi."
                    ]
                },
                new DetailSection
                {
                    Heading = "Reading on your devices",
                    BulletList =
                    [
                        "<strong>Browser</strong> — Open <a href=\"http://books.lan\" target=\"_blank\">books.lan</a> and use the built-in EPUB reader.",
                        "<strong>Download</strong> — Download any book and open in Moon+ Reader, KOReader, Apple Books, etc.",
                        "<strong>Kindle</strong> — Set your Kindle email in your profile settings and use the \"Send to Kindle\" button.",
                        "<strong>OPDS</strong> — Compatible OPDS clients (Kybook, PocketBook) can subscribe to <code>books.lan/opds</code>."
                    ]
                },
                new DetailSection
                {
                    Heading = "Searching & browsing",
                    Paragraphs =
                    [
                        "Books are searchable by title, author, series, tag, and language. The shelf view shows recently added books on the home page. Use the sidebar filters to browse by genre or format."
                    ]
                },
                new DetailSection
                {
                    Heading = "Adding books",
                    Paragraphs =
                    [
                        "Use the full Calibre desktop app (on any machine with the library mounted or synced) to add, tag, and edit metadata. Calibre-Web reads the library database directly — changes in Calibre appear in Calibre-Web after a library refresh."
                    ],
                    CodeBlock = "Library path: /mnt/data/calibre-library/\nDatabase:     metadata.db (managed by Calibre)"
                }
            ]
        },

        new ServiceInfo
        {
            Slug = "tailscale",
            Name = "Tailscale",
            Icon = "🌐",
            Host = "WAN Access · VPN Mesh",
            LaunchUrl = null,
            AccentToken = "teal",
            Status = ServiceStatus.Connected,
            CardDescription = "Zero-config VPN mesh. Punch through NAT from anywhere in the world and access the full LAN as if you're home. Runs as a sidecar — no separate UI needed.",
            HeroDescription = "Zero-config WireGuard VPN mesh. Access The Hold from anywhere in the world as if you're home.",
            Sections =
            [
                new DetailSection
                {
                    Heading = "What it does",
                    Paragraphs =
                    [
                        "Tailscale creates a private mesh network between all your devices using WireGuard under the hood. Once connected, your phone or laptop can reach the Pi's LAN IP directly — even through a hotel Wi-Fi, cellular, or any NAT — without any port forwarding on the router. It just works.",
                        "The Pi runs Tailscale as a sidecar container and advertises itself as a subnet router, exposing the full <code>192.168.1.0/24</code> LAN to connected devices."
                    ]
                },
                new DetailSection
                {
                    Heading = "Connecting a new device",
                    NumberedList =
                    [
                        "Install the <strong>Tailscale</strong> app from tailscale.com or your app store.",
                        "Sign in with the shared Tailscale account credentials (ask Jordan).",
                        "Once connected, you'll get a <code>100.x.x.x</code> Tailscale IP for each device.",
                        "Access services via the Pi's Tailscale IP or by using the Pi as a subnet router."
                    ],
                    TipLabel = "Subnet routing:",
                    TipBody = "If the Pi is configured as a subnet router, you can reach all <code>192.168.1.x</code> addresses (and thus all <code>*.lan</code> names, if you also set Tailscale DNS) remotely without changing anything."
                },
                new DetailSection
                {
                    Heading = "Using *.lan names remotely",
                    Paragraphs = ["To resolve <code>*.lan</code> hostnames over Tailscale, either:"],
                    BulletList =
                    [
                        "<strong>MagicDNS + nameserver override</strong>: Set the Pi's Tailscale IP as a custom nameserver in the Tailscale admin console for your tailnet, pointing <code>lan</code> queries to Pi-hole.",
                        "<strong>Direct IP</strong>: Use the Pi's <code>100.x.x.x</code> Tailscale IP directly in browser: <code>http://100.x.x.x:80</code> with a Host header override (less convenient)."
                    ]
                },
                new DetailSection
                {
                    Heading = "Checking connection status",
                    CodeBlock = "docker exec tailscale tailscale status"
                }
            ]
        },

        new ServiceInfo
        {
            Slug = "joplin",
            Name = "Joplin",
            Icon = "📝",
            Host = "notes.lan",
            LaunchUrl = "http://notes.lan",
            AccentToken = "coral",
            Status = ServiceStatus.Running,
            CardDescription = "Shared markdown note-taking and list sync server. Jordan and the Admiral sync notes, to-dos, and lists across all devices — no Evernote, no Notion, no cloud.",
            HeroDescription = "Shared markdown notes, to-dos, and lists for the Flomads — synced across all devices, no cloud.",
            Sections =
            [
                new DetailSection
                {
                    Heading = "What it does",
                    Paragraphs =
                    [
                        "Joplin Server is the sync backend for Joplin, the open-source markdown note-taking app. Both Jordan and the Admiral run the Joplin app on their phones and laptops, and all notes sync through this private server. Shared notebooks are visible to both users — perfect for shared to-do lists, trip notes, grocery lists, and research."
                    ]
                },
                new DetailSection
                {
                    Heading = "Setting up a new device",
                    NumberedList =
                    [
                        "Install <strong>Joplin</strong> from joplinapp.org or your app store (it's free).",
                        "Go to <strong>Settings → Synchronisation</strong>.",
                        "Set Sync Target to <strong>Joplin Server</strong>.",
                        "Server URL: <code>http://notes.lan</code>",
                        "Enter your username and password (ask Jordan for credentials).",
                        "Tap Synchronise — your notes will download in seconds."
                    ],
                    TipLabel = "Away from home?",
                    TipBody = "Set the server URL to the Pi's Tailscale IP instead of <code>notes.lan</code> and sync works over VPN from anywhere."
                },
                new DetailSection
                {
                    Heading = "Shared notebooks",
                    Paragraphs =
                    [
                        "To share a notebook with the other user, right-click (or long-press) the notebook and choose <strong>Share notebook</strong>. Enter the other user's email address. Changes sync in both directions — great for collaborative lists."
                    ]
                },
                new DetailSection
                {
                    Heading = "Tips for the Admiral",
                    BulletList =
                    [
                        "Use the <strong>To-do</strong> note type (checkbox icon) for lists — items can be checked off on any device.",
                        "Notes support full Markdown: headers, bold, italics, tables, links.",
                        "Attach photos directly to notes — they sync to all devices.",
                        "The web interface at <a href=\"http://notes.lan\" target=\"_blank\">notes.lan</a> works fine in a browser if you don't want the app."
                    ]
                }
            ]
        },

        new ServiceInfo
        {
            Slug = "nextcloud",
            Name = "Nextcloud",
            Icon = "☁️",
            Host = "cloud.lan",
            LaunchUrl = "http://cloud.lan",
            AccentToken = "teal",
            Status = ServiceStatus.Running,
            CardDescription = "The de-Googled cloud. File sync, calendar, contacts, and more. Your private Dropbox, Google Drive, and Google Calendar combined — entirely self-hosted.",
            HeroDescription = "Your de-Googled cloud. Files, calendar, contacts, and more — entirely self-hosted.",
            Sections =
            [
                new DetailSection
                {
                    Heading = "What it does",
                    Paragraphs =
                    [
                        "Nextcloud is the Swiss Army knife of self-hosting. It replaces Google Drive (file sync), Google Calendar, Google Contacts, and Google Photos — all from your Pi. Files sync across devices via the desktop and mobile apps, calendars and contacts sync via CalDAV/CardDAV, and the web interface provides a full file manager."
                    ]
                },
                new DetailSection
                {
                    Heading = "Mobile setup — Files",
                    NumberedList =
                    [
                        "Install the <strong>Nextcloud</strong> app (iOS/Android).",
                        "Server URL: <code>http://cloud.lan</code>",
                        "Log in with your credentials.",
                        "Enable <strong>Auto Upload</strong> in the app to automatically back up photos (though Immich does this better)."
                    ]
                },
                new DetailSection
                {
                    Heading = "Calendar & Contacts sync",
                    Paragraphs = ["Nextcloud supports CalDAV and CardDAV, which means your phone's native calendar and contacts apps can sync to it directly."],
                    CodeBlock = "# CalDAV (Calendar)\nServer: http://cloud.lan/remote.php/dav\n\n# CardDAV (Contacts)\nServer: http://cloud.lan/remote.php/dav",
                    TipLabel = "iOS:",
                    TipBody = "Settings → Mail → Accounts → Add Account → Other → Add CalDAV Account. Use the URL above and your Nextcloud credentials."
                },
                new DetailSection
                {
                    Heading = "Nextcloud apps to enable",
                    BulletList =
                    [
                        "<strong>Calendar</strong> — Full calendar app in the browser.",
                        "<strong>Contacts</strong> — Address book management.",
                        "<strong>Tasks</strong> — To-do lists that sync via CalDAV.",
                        "<strong>Notes</strong> — Simple markdown notes (lighter than Joplin).",
                        "<strong>Talk</strong> — Private chat and video calls on-network."
                    ]
                }
            ]
        },

        new ServiceInfo
        {
            Slug = "syncthing",
            Name = "Syncthing",
            Icon = "🔄",
            Host = "sync.lan",
            LaunchUrl = "http://sync.lan",
            AccentToken = "green",
            Status = ServiceStatus.Running,
            CardDescription = "Continuous peer-to-peer file synchronization. Keeps folders in sync across all your devices over LAN and Tailscale. No server required — just direct sync.",
            HeroDescription = "Continuous, decentralized file sync across all devices — peer-to-peer, no server required.",
            Sections =
            [
                new DetailSection
                {
                    Heading = "What it does",
                    Paragraphs =
                    [
                        "Syncthing synchronizes folders directly between devices without any cloud intermediary. The Pi instance acts as an always-on anchor node — since it's always running, your other devices sync to it whenever they're on the network, keeping everything up to date even when your laptop is closed. It's like Dropbox, but the server is your Pi and the protocol is open and encrypted."
                    ]
                },
                new DetailSection
                {
                    Heading = "Adding a new device",
                    NumberedList =
                    [
                        "Install Syncthing on the new device (syncthing.net).",
                        "Open the Syncthing web UI on the new device and copy its <strong>Device ID</strong>.",
                        "Open <a href=\"http://sync.lan\" target=\"_blank\">sync.lan</a> on the Pi UI.",
                        "Click <strong>Add Remote Device</strong> and paste the Device ID.",
                        "Accept the connection request on the new device.",
                        "Share the desired folder(s) with the new device from the Pi's folder settings."
                    ]
                },
                new DetailSection
                {
                    Heading = "Typical synced folders",
                    BulletList =
                    [
                        "<strong>Documents</strong> — shared documents and working files.",
                        "<strong>eBooks</strong> — Calibre library folder, keeping it in sync across machines.",
                        "<strong>Photos-export</strong> — curated exports from Immich.",
                        "<strong>Config-backups</strong> — Docker compose files, dot-files, and configs."
                    ]
                },
                new DetailSection
                {
                    Heading = "Over Tailscale",
                    Paragraphs =
                    [
                        "Syncthing works over Tailscale automatically — devices see each other's Tailscale IPs and sync wherever they are. No extra config needed; Syncthing discovers peers via the shared relay infrastructure and then communicates directly."
                    ]
                }
            ]
        },

        new ServiceInfo
        {
            Slug = "immich",
            Name = "Immich",
            Icon = "📷",
            Host = "photos.lan",
            LaunchUrl = "http://photos.lan",
            AccentToken = "gold",
            Status = ServiceStatus.Running,
            CardDescription = "High-performance self-hosted photo and video backup. Face recognition, map view, albums, and mobile apps with automatic backup — no Google Photos, ever again.",
            HeroDescription = "High-performance self-hosted photo and video backup with face recognition, map view, and mobile apps.",
            Sections =
            [
                new DetailSection
                {
                    Heading = "What it does",
                    Paragraphs =
                    [
                        "Immich is your private Google Photos replacement. The mobile app automatically backs up every photo and video on your phone to the Pi. From there, you get face recognition, location map view, smart albums, sharing, and a beautiful timeline UI — all without a single photo leaving your network."
                    ]
                },
                new DetailSection
                {
                    Heading = "Setting up the mobile app",
                    NumberedList =
                    [
                        "Install <strong>Immich</strong> from your app store.",
                        "Server URL: <code>http://photos.lan</code> (on home Wi-Fi) or Tailscale IP when away.",
                        "Log in with your credentials.",
                        "Go to <strong>Settings → Background backup</strong> and enable it.",
                        "Your photos will start uploading immediately. First backup may take a while!"
                    ],
                    TipLabel = "Tip for the Admiral:",
                    TipBody = "Enable <em>Backup over mobile data</em> in the app settings if you want photos backed up even when you're not on Wi-Fi (uses mobile data). Otherwise backup happens automatically on home Wi-Fi."
                },
                new DetailSection
                {
                    Heading = "Shared albums",
                    Paragraphs =
                    [
                        "Both Jordan and the Admiral can create shared albums visible to the other user. Perfect for trip collections — create an album for \"Shropshire 2026\" and add to it from either phone."
                    ]
                },
                new DetailSection
                {
                    Heading = "Memory Lane",
                    Paragraphs =
                    [
                        "Immich surfaces \"On This Day\" memories from previous years — a lovely feature for full-time travelers with years of photos from around the world."
                    ]
                },
                new DetailSection
                {
                    Heading = "Storage",
                    CodeBlock = "Photos stored at: /mnt/data/immich/\nThumbnails:      /mnt/data/immich/thumbs/\nEncoded video:   /mnt/data/immich/encoded-video/"
                }
            ]
        },

        new ServiceInfo
        {
            Slug = "whiteboard",
            Name = "Whiteboard",
            Icon = "🖊️",
            Host = "draw.lan",
            LaunchUrl = "http://draw.lan",
            AccentToken = "coral",
            Status = ServiceStatus.Running,
            CardDescription = "rofl256/whiteboard — a clean, collaborative infinite canvas for sketching, brainstorming, and drawing together in real time on the local network.",
            HeroDescription = "Simple, collaborative infinite canvas for sketching and real-time drawing together on the LAN.",
            Sections =
            [
                new DetailSection
                {
                    Heading = "What it does",
                    Paragraphs =
                    [
                        "The rofl256/whiteboard container provides a clean, minimal infinite drawing canvas accessible from any browser. Multiple people on the same network can draw on the same canvas in real time — no accounts, no setup, just open and draw. Works great with a stylus on a tablet."
                    ]
                },
                new DetailSection
                {
                    Heading = "Using it",
                    BulletList =
                    [
                        "Open <a href=\"http://draw.lan\" target=\"_blank\">draw.lan</a> on any device on the network.",
                        "Draw with mouse, trackpad, or stylus — pressure sensitivity supported on compatible devices.",
                        "Multiple users can join the same session simultaneously.",
                        "Use the toolbar for pen, shapes, text, eraser, and color picker.",
                        "Export the canvas as a PNG or SVG from the toolbar."
                    ],
                    TipLabel = "On a phone:",
                    TipBody = "Works best in landscape mode with a stylus. For brainstorming sessions, try splitting the screen — whiteboard on one side, notes on the other."
                },
                new DetailSection
                {
                    Heading = "Use cases",
                    BulletList =
                    [
                        "Finca design sketching",
                        "Architecture and floor plan drafts",
                        "Mind mapping and brainstorming",
                        "Quick diagrams and flow charts",
                        "Collaborative note-taking during calls"
                    ]
                }
            ]
        },

        new ServiceInfo
        {
            Slug = "spacedeck",
            Name = "Spacedeck",
            Icon = "🗂️",
            Host = "spacedeck.lan",
            LaunchUrl = "http://spacedeck.lan",
            AccentToken = "purple",
            Status = ServiceStatus.Running,
            CardDescription = "Spacedeck Open — a visual presentation and collaboration tool. Build spatial, zoomable decks with text, images, and drawings. Your private Miro.",
            HeroDescription = "A zoomable visual canvas for spatial presentations, decks, and collaborative thinking — your private Miro.",
            Sections =
            [
                new DetailSection
                {
                    Heading = "What it does",
                    Paragraphs =
                    [
                        "Spacedeck Open is a visual collaboration tool where you build spatial \"spaces\" — zoomable canvases containing text cards, images, drawings, and embedded content. Unlike a standard whiteboard, Spacedeck is organized and navigable, making it great for building visual knowledge bases, mood boards, project overviews, and presentations you can walk through."
                    ]
                },
                new DetailSection
                {
                    Heading = "Getting started",
                    NumberedList =
                    [
                        "Open <a href=\"http://spacedeck.lan\" target=\"_blank\">spacedeck.lan</a> and log in (or register a local account).",
                        "Create a new Space (it's an infinite canvas).",
                        "Double-click to add a text card. Drag images in. Draw with the pencil tool.",
                        "Use the navigator in the corner to zoom to different areas of your space.",
                        "Share the space URL with anyone on the LAN for real-time collaboration."
                    ]
                },
                new DetailSection
                {
                    Heading = "Compared to Whiteboard",
                    Paragraphs =
                    [
                        "Use <strong>Whiteboard</strong> (draw.lan) for quick freehand sketches and real-time collaborative drawing. Use <strong>Spacedeck</strong> (spacedeck.lan) when you want something more structured — organized cards, a navigable layout, saved named spaces, and richer content types."
                    ]
                },
                new DetailSection
                {
                    Heading = "Use cases",
                    BulletList =
                    [
                        "Finca community design — zone maps, project cards, and vision boards.",
                        "Travel planning — destinations, logistics, photos, and notes on one canvas.",
                        "Worldbuilding and creative projects.",
                        "Visual project management without the subscription."
                    ]
                }
            ]
        }
    ];
}
