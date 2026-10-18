# NetTunnel configuration guide

How to configure NetTunnel for **Raw TCP**, **HTTP** and **HTTPS** traffic, what every setting does, and general guidance for running it.

Contents

1. [How the pieces fit together](#1-how-the-pieces-fit-together)
2. [Endpoint settings](#2-endpoint-settings)
3. [Choosing traffic types](#3-choosing-traffic-types)
4. [Raw (plain TCP)](#4-raw-plain-tcp)
5. [HTTP](#5-http)
6. [HTTPS (TLS termination and re-encryption)](#6-https-tls-termination-and-re-encryption)
7. [HTTP header rules](#7-http-header-rules)
8. [Service configuration](#8-service-configuration)
9. [Where configuration is stored](#9-where-configuration-is-stored)
10. [General guidance](#10-general-guidance)
11. [Troubleshooting](#11-troubleshooting)
12. [Known limitations](#12-known-limitations)

---

## 1. How the pieces fit together

NetTunnel runs as a service on each machine. Two services are joined by a **tunnel**, an encrypted, compressed connection that is always opened by the **outbound tunnel** side (the one behind the firewall) to the **inbound tunnel** side (the one that can accept connections). Management and all tunneled data share that one connection.

An **endpoint** is a port-forward that runs through a tunnel. Each endpoint has two ends:

| End | What it does | Uses |
| --- | --- | --- |
| **Inbound endpoint** | Listens on a port and accepts connections from clients. | `InboundPort`, `InboundTrafficType`, the TLS certificate |
| **Outbound endpoint** | Opens a connection to the destination for every client connection. | `OutboundAddress`, `OutboundPort`, `OutboundTrafficType` |

```
 client ──► [ inbound endpoint ]══ tunnel (encrypted, compressed) ══[ outbound endpoint ] ──► destination
            listens on InboundPort                                    connects to OutboundAddress:OutboundPort
            TLS terminated here (HTTPS)                               TLS re-encrypted here (HTTPS)
            Request header rules applied here                         Response header rules applied here
```

Things to know:

- **The same endpoint configuration is stored at both services.** The service that listens holds it with direction *Inbound*, the other with direction *Outbound* (the direction is flipped when it is sent to the peer). Each end only reads the fields that apply to it, so one dialog configures both.
- **The inbound end can be on either service.** "Add inbound endpoint" makes the service you are managing the listener and its peer the one that connects to the destination. "Add outbound endpoint" does the reverse. Pick whichever places the listener where your clients are and the destination where the destination is.
- **Endpoints belong to a user account.** The user a tunnel logs in as decides which endpoints it receives. Create separate users to give different tunnels different endpoint sets.
- **Endpoints can be added in two places**: on a connected tunnel (Endpoints pane in the main window) or on a user (Users dialog). They are the same thing; the user list is what is sent to a tunnel when it connects.
- Data is protected by the tunnel's own encryption while it travels between the two services, whatever the traffic type. The traffic type only describes what is spoken at each *edge* (client to inbound endpoint, outbound endpoint to destination).

## 2. Endpoint settings

The endpoint dialog has three tabs.

### Endpoint tab

| Setting | Meaning |
| --- | --- |
| **Name or description** | Your own label. Required. |
| **Listen port** | Port the inbound endpoint listens on (1-65535). It listens on all network interfaces. |
| **Termination address / host** | Host name, domain or IP the outbound endpoint connects to. Required. |
| **Termination port** | Port on the destination (1-65535). |

### HTTP Headers tab

| Setting | Meaning |
| --- | --- |
| **Inbound traffic type** | Raw, HTTP or HTTPS, as spoken by clients connecting to the inbound endpoint. |
| **Outbound traffic type** | Raw, HTTP or HTTPS, as spoken to the destination. |
| **Header rules grid** | HTTP header changes (see [section 7](#7-http-header-rules)). Applies to HTTP and HTTPS only. |

### TLS tab

| Setting | Meaning |
| --- | --- |
| **Certificate (PFX) path on the inbound host** | PKCS#12 file the inbound endpoint presents to clients. Required when the inbound type is HTTPS. The path is read by the service that hosts the inbound endpoint, on that machine. |
| **Certificate password** | Password for the PFX. Leave empty if it has none. |
| **Ignore certificate errors from the destination server** | Outbound HTTPS only. Skips validation of the destination's certificate. |
| **Offer HTTP/2 to clients (the destination must support it)** | Inbound HTTPS only. Lets clients that support HTTP/2 use it. See [HTTP/2](#http2). |

## 3. Choosing traffic types

The two types are independent, so every combination is allowed. The ones people use:

| Inbound | Outbound | Use it for | Header rules | Needs a PFX |
| --- | --- | --- | --- | --- |
| Raw | Raw | Any TCP protocol (SSH, RDP, databases, MQTT) | No | No |
| HTTP | HTTP | Plain web traffic | Yes | No |
| HTTPS | HTTP | Put HTTPS in front of a plain HTTP server | Yes | Yes |
| HTTPS | HTTPS | HTTPS in, HTTPS to the backend (re-encrypt) | Yes | Yes |
| HTTP | HTTPS | Plain in, HTTPS to the backend | Yes | No |
| Raw | Raw (carrying TLS) | Pass an end-to-end TLS session through untouched | No | No |

The deciding question is whether NetTunnel has to **read** the HTTP. Header rules can only be applied to HTTP that NetTunnel can see, which means HTTP, or HTTPS that NetTunnel terminates itself. If you only need to move bytes, use Raw.

## 4. Raw (plain TCP)

Raw copies bytes in both directions and never looks at them. It works for any protocol.

Example: reach an SSH server in an office network from home.

1. Connect the tunnel (the office service makes an outbound tunnel to the home service).
2. On the home service, add an **inbound** endpoint:
   - Listen port `2222`
   - Termination address `192.168.1.10` (the SSH server, as seen from the office)
   - Termination port `22`
   - Inbound traffic type `Raw`, outbound traffic type `Raw`
3. Connect with `ssh -p 2222 user@home-machine`.

Raw is also how you carry TLS **end to end**: if the client and the destination negotiate TLS between themselves, NetTunnel just forwards the encrypted bytes. The certificate is then the destination's, not NetTunnel's, and header rules do not apply.

## 5. HTTP

Set both types to **HTTP** to enable header rules on plain HTTP/1.x traffic. Nothing is encrypted at the edges, so use it only between machines you trust (it is still encrypted between the two services).

Example: expose an internal intranet site, rewriting the `Host` header the site expects.

- Listen port `8080`, termination `intranet.local:80`
- Inbound `HTTP`, outbound `HTTP`
- Rule: **Request**, **Any**, header `Host`, action **Upsert**, value `intranet.local`

## 6. HTTPS (TLS termination and re-encryption)

NetTunnel can terminate TLS at the inbound endpoint (so it can read and change the HTTP) and re-encrypt on the way out to the destination. The two halves are configured separately.

### Inbound: terminate TLS (needs a certificate)

1. Set **Inbound traffic type** to `HTTPS`.
2. On the **TLS** tab, set the **PFX path** and **password**.
3. Save. The certificate is loaded when the endpoint starts. If the file can't be read, the endpoint does not listen and the service logs the reason.

The certificate must:

- contain the **private key** (a `.cer` or `.crt` alone does not work);
- be issued for the **host name clients type** (CN or SAN). Browsing by IP address or `localhost` will not match;
- be issued by an authority your clients trust, or clients show a warning.

If you have no certificate:

| Situation | Option |
| --- | --- |
| A public host name | A free [Let's Encrypt](https://letsencrypt.org) certificate. If the endpoint is not on port 80/443 or is behind a router, use the DNS challenge (for example with [Posh-ACME](https://poshac.me): `New-PACertificate host.example.com -AcceptTOS -Contact you@example.com -Plugin Manual`, then use the `fullchain.pfx` it writes; its default password is `poshacme`). |
| One machine, local testing | [mkcert](https://github.com/FiloSottile/mkcert): `mkcert -install` then `mkcert -pkcs12 host.example.com`. |
| Internal machines | An internal CA, or a self-signed certificate imported into each client's trusted store. |

Use the **production** Let's Encrypt server for real use. Certificates from its staging server are deliberately not trusted by browsers.

Certificates expire. The PFX is read when the endpoint starts, so after replacing the file, **save the endpoint again (or restart the service)** to load the new one.

### Outbound: re-encrypt to the destination (no certificate)

Set **Outbound traffic type** to `HTTPS`. NetTunnel opens a TLS connection to the destination, which presents its own certificate. No PFX is involved on this side.

- The certificate is validated against the machine's trusted authorities and against the **termination address**. Use the destination's **host name**, not its IP address, or validation fails on the name.
- For a destination with a self-signed or internal-CA certificate, either trust that CA on the machine running the outbound endpoint, or tick **Ignore certificate errors from the destination server**. Ticking it removes protection against an impersonated destination, so avoid it over untrusted networks.
- Client certificates (mutual TLS) are not supported in either direction.

### Example: HTTPS in front of a site, re-encrypting to the real server

| Setting | Value |
| --- | --- |
| Listen port | `9443` |
| Termination address / port | `www.example.com` / `443` |
| Inbound traffic type | HTTPS |
| Outbound traffic type | HTTPS |
| PFX path / password | the certificate for the name clients use |
| Rule | Request, Any, `Host`, Upsert, `www.example.com` |

The `Host` rule matters here: the destination serves its site by `Host`, and the browser will send the name it was given (for example `nettunnel.example.com:9443`).

### Notes

- HTTP/2 is off unless you turn it on (see [HTTP/2](#http2)); without it clients are served HTTP/1.1. HTTP/3 is not supported.
- One certificate per endpoint. To serve several names on different certificates, create several endpoints on different ports.
- The TLS handshake must finish within 30 seconds or the connection is dropped.
- Handshake failures from browsers (for example a connection opened and abandoned, or a rejected certificate) are logged as one short line at Verbose or Warning level, not as errors.

### HTTP/2

Tick **Offer HTTP/2 to clients** on the TLS tab to let browsers and other clients use HTTP/2. It only applies when the inbound traffic type is HTTPS, because HTTP/2 is negotiated during the TLS handshake. It is off by default.

**The destination must speak HTTP/2 too.** NetTunnel relays the connection, it does not translate between protocol versions. A client that negotiates HTTP/2 is connected to the destination using HTTP/2:

- With **outbound HTTPS**, NetTunnel asks the destination for HTTP/2 during the handshake. If the destination does not support it, the connection fails and the log says so. Clients that do not use HTTP/2 are connected using HTTP/1.1 as before, so those keep working.
- With **outbound HTTP**, HTTP/2 is sent as plain-text HTTP/2 ("h2c" with prior knowledge). Only use that with a destination that is set up for it (gRPC servers usually are).

Turn it on only for destinations you know support HTTP/2, and leave it off otherwise.

Header rules work on HTTP/2 as they do on HTTP/1.1, with these differences:

- A rule for `Host` is applied to the request's `:authority`. Header names are compared without regard to case and are written in lower case.
- The connection-specific headers (`Connection`, `Keep-Alive`, `Proxy-Connection`, `Transfer-Encoding`, `Upgrade`) are not allowed in HTTP/2, so a rule cannot add or change them. A rule can still delete one.
- Pseudo-headers (`:path`, `:method`, `:scheme`, `:status`) can be changed or deleted by naming them in a rule, but a rule cannot add one (except `:authority`, through `Host`). Take care: deleting one produces a request or response that is not valid.
- Rules apply to a request, or to the first non-informational response, of each stream. Trailers and `1xx` responses are passed on unchanged.
- Bodies, and everything else in the HTTP/2 connection (settings, pings, flow control), are passed through untouched. If there are no enabled rules for an end, HTTP/2 is relayed byte for byte, without being parsed.
- If the stream is not valid HTTP/2, the connection is closed (it cannot be passed through unchanged part-way, because of how headers are compressed).

If you do not need header rules and only want HTTP/2 to work end to end, you can also use **Raw** on both ends and let the client and destination negotiate it between themselves (the certificate is then the destination's).

## 7. HTTP header rules

Rules edit the HTTP **header** (the request or status line and the header fields) as it passes. Bodies are never changed. They apply to the **HTTP** and **HTTPS** traffic types and are ignored for Raw.

Each rule has these columns:

| Column | Values | Meaning |
| --- | --- | --- |
| **Enabled** | True / False | Disabled rules are kept but not applied. |
| **Type** | Request, Response, Any | Which messages the rule applies to. |
| **Verb** | Any, Connect, Delete, Get, Head, Options, Post, Put, Patch, Trace | The request method to match. |
| **Header** | text | Name of the header field (not case sensitive). |
| **Action** | Insert, Update, Upsert, Delete | What to do (below). |
| **Value** | text | The value to write. Ignored by Delete. |

### Actions

| Action | If the header exists | If it does not |
| --- | --- | --- |
| **Insert** | Left alone | Added |
| **Update** | Value replaced | Nothing happens |
| **Upsert** | Value replaced | Added |
| **Delete** | The whole line is removed | Nothing happens |

Rules run in the order they are listed. Values are written exactly as typed. There are no variables, so a rule cannot insert the client's address or copy another header.

### Where rules take effect

Each end reads one direction of the conversation:

- The **inbound endpoint** reads **requests** from clients, so **Request** rules take effect there.
- The **outbound endpoint** reads **responses** from the destination, so **Response** rules take effect there.
- **Any** takes effect at both: it edits the request at the inbound end and the response at the outbound end.

Consequences:

- A **Response** rule must use the verb **Any**. A response does not carry the request's method, so a rule for a specific verb could never match. The dialog refuses to save it otherwise.
- A Request rule only matches requests that the inbound endpoint sees, so it has no effect when the inbound traffic type is Raw. Likewise a Response rule needs the outbound type to be HTTP or HTTPS.
- A warning is logged if a response is seen on an inbound endpoint, or a request on an outbound endpoint (typically a misconfigured pairing).

### Common recipes

| Goal | Type | Verb | Header | Action | Value |
| --- | --- | --- | --- | --- | --- |
| Make the backend see its own host name | Request | Any | `Host` | Upsert | `backend.example.com` |
| Tell the backend the client used HTTPS | Request | Any | `X-Forwarded-Proto` | Upsert | `https` |
| Hide the backend's software | Response | Any | `Server` | Delete | |
| Hide a framework banner | Response | Any | `X-Powered-By` | Delete | |
| Require browsers to keep using HTTPS | Response | Any | `Strict-Transport-Security` | Upsert | `max-age=31536000` |
| Stop caching of every response | Response | Any | `Cache-Control` | Upsert | `no-store` |
| Remove cookies on one method | Request | Post | `Cookie` | Delete | |
| Add a header only when the client sent none | Request | Any | `Accept-Language` | Insert | `en-US` |

### Behaviours worth knowing

- Rules are applied to **every message** on a keep-alive connection, not just the first. The parser follows `Content-Length` and chunked bodies so body bytes are never mistaken for headers.
- If the stream stops looking like HTTP (a WebSocket or other upgrade, `CONNECT`, a body delimited only by closing the connection, or non-HTTP data) the rest of that connection is passed through unchanged and rules stop applying.
- Headers larger than 64 KB are not modified; the connection is passed through.
- A rule that rewrites `Host` does not rewrite redirects. A backend that answers `Location: https://backend.example.com/...` still sends clients to that name.

## 8. Service configuration

Open **Settings > Service configuration** (administrators only). Each service has its own configuration.

| Setting | Default | Allowed | Meaning |
| --- | --- | --- | --- |
| **Management port** (`ServicePort`) | 52845 | 1-65535 | Port the service accepts UI and tunnel connections on. Restart the service after changing. |
| **Message query timeout** (`MessageQueryTimeoutMs`) | 10000 | 1000-3600000 | Milliseconds to wait on a query to the peer. |
| **Endpoint heartbeat** (`EndpointHeartbeatDelayMs`) | 5000 | 1000-216000000 | Milliseconds between endpoint heartbeats, which also check for stale connections. |
| **Tunnel cryptography key size** (`TunnelCryptographyKeySize`) | 4096 | 128-81920 | Bits of key material negotiated per tunnel at each connect. |
| **Stale endpoint expiration** (`StaleEndpointExpirationMs`) | 0 (off) | 0-216000000 | Disconnect an endpoint connection after this many idle milliseconds. |
| **Initial receive buffer** (`InitialReceiveBufferSize`) | 16384 | 1024-1073741824 | Starting size, in bytes, of each connection's receive buffer. |
| **Maximum receive buffer** (`MaxReceiveBufferSize`) | 1048576 | 1024-1073741824 | Largest the buffer grows to. |
| **Receive buffer growth rate** (`ReceiveBufferGrowthRate`) | 0.2 | 0.01-1.0 | How fast the buffer grows when it fills. |
| **Ping cadence** (`PingCadence`) | 5000 | 0-1073741824 | Milliseconds between pings to the peer. 0 turns pinging off. |
| **Log level** (`LogLevel`) | Verbose | Exception, Warning, Verbose, Debug | Most detailed level recorded. |
| **Log file path** (`LogPath`) | empty | path | Log file on the service machine. Relative paths are relative to the install folder. Empty turns the file off. |
| **Log file roll-over size** (`LogFileMaxSizeMB`) | 10 | 1-10240 | Size at which `NetTunnel.log` rolls to `NetTunnel.1.log`, and so on. |
| **Rolled log files to keep** (`LogFileRetainedCount`) | 5 | 0-1000 | Old files kept besides the current one. |
| **Write warnings and exceptions to the event log** (`LogToEventLog`) | on | on / off | Also writes them to the Windows Application event log. |

The log settings apply as soon as you save. For the others, restart the service to be sure the change is in effect. The `ServiceId` identifies the instance and its tunnels; it is kept when settings are saved and should not be edited.

Tune the buffers only if you move large volumes: a bigger initial buffer reads more per call but uses more memory per connection.

## 9. Where configuration is stored

Everything is in the service's common application data folder, as JSON:

| Platform | Folder |
| --- | --- |
| Windows | `C:\ProgramData\NetTunnel\` |
| Linux | `/usr/share/NetTunnel/` |

| File | Contents |
| --- | --- |
| `ServiceConfiguration.json` | The service settings from [section 8](#8-service-configuration). |
| `TunnelConfiguration.json` | The tunnels this service connects out to (name, address, port, user, password hash). |
| `User.json` | Users, roles and their **endpoints**, including header rules and TLS settings. |

Use the UI to make changes. If you edit a file by hand, **stop the service first**, because it rewrites the files when it saves. Enumerations are stored as text (`"Https"`, `"Upsert"`). Endpoint ids are regenerated every time a file is loaded. [Secrets](#secrets-in-the-files) are stored encrypted, which ties them to the machine that wrote them.

Example endpoint as stored inside a user in `User.json`:

```json
{
  "Direction": "Outbound",
  "Name": "Website Redirector Endpoint",
  "OutboundAddress": "www.example.com",
  "InboundPort": 9443,
  "OutboundPort": 443,
  "InboundTrafficType": "Https",
  "OutboundTrafficType": "Https",
  "HttpHeaderRules": [
    {
      "HeaderType": "Request",
      "Action": "Upsert",
      "Verb": "Any",
      "Enabled": true,
      "Name": "Host",
      "Value": "www.example.com",
      "Description": ""
    }
  ],
  "TlsCertificatePath": "C:\\certs\\nettunnel.pfx",
  "TlsCertificatePassword": "enc:dpapi:AQAAANCMnd8...",
  "TlsIgnoreBackendCertificateErrors": false,
  "AllowHttp2": false
}
```

> Older files that have a single `TrafficType` value are not understood any more. Both traffic types now default to `Raw`; set `InboundTrafficType` and `OutboundTrafficType` instead.

### Secrets in the files

The secrets in the files are stored encrypted, so they are not readable if a file is copied, emailed or ends up in a backup:

- the **PFX password** of each endpoint (`TlsCertificatePassword` in `User.json`),
- the **password hashes** of the users (`User.json`) and of the tunnels (`TunnelConfiguration.json`).

A protected value looks like `"enc:dpapi:AQAAANCMnd8..."`. On **Windows** it is encrypted with DPAPI using the machine key, so it works whichever account the service runs as, and only on that machine. On **Linux** it is encrypted with AES-256-GCM using a random key kept in `secret.key` next to the configuration files, created on first use and readable only by its owner (`/usr/share/NetTunnel/secret.key`). Back that key file up with the configuration or the secrets cannot be recovered.

Things to know:

- **Files from an older version, or values you type in by hand, are plain text.** They are read as they are and are encrypted the next time the service saves, so you can upgrade in place, or put a plain `"TlsCertificatePassword": "..."` into a file to set it.
- **Copying a configuration to another machine:** the encrypted values do not decrypt there. The service logs a warning for each one and treats it as empty (a user with an unreadable password hash cannot log in). To move a configuration, replace the `enc:...` values with the plain values before copying, and they are encrypted again on the new machine.
- **What this protects against:** the file leaking, not a person who can run programs as an administrator (or as the service account) on the same machine, who can decrypt them the same way the service does. The PFX itself is not encrypted by NetTunnel: it is protected by its own password. Keep it, and the folder `C:\ProgramData\NetTunnel\`, readable only by administrators and the service account.
- Password hashes are plain SHA-256 of the password, so a strong password matters.

## 10. General guidance

**Security**

- **Change the default login first.** A new installation creates the user `root` with the machine's lower-case name as its password. Change it from the Users dialog before exposing the service.
- Only the **management port** (default 52845) needs to be reachable on the service that accepts tunnels. Endpoint ports are only needed where clients connect. Open nothing else.
- Endpoint listeners bind to **all network interfaces**. Restrict who can reach them with the host firewall.
- With HTTP or Raw edges, traffic is in the clear between the client and the inbound endpoint and between the outbound endpoint and the destination. Keep those hops on trusted networks, or use HTTPS at the edges.
- Prefer a trusted certificate with the default validation over **Ignore certificate errors**.
- Passwords are encrypted in the configuration files ([Secrets in the files](#secrets-in-the-files)). Back up the `secret.key` file on Linux, and keep the configuration folder readable only by administrators and the service account.
- Use the lowest-privilege users that fit. Administrators can change service configuration and manage users; give a tunnel a dedicated user whose endpoints are only the ones it needs.

**Design**

- Make the service that is **hard to reach** (behind a firewall or NAT) the outbound tunnel side. It dials out, so no inbound firewall rule is needed there.
- Put the **inbound endpoint where the clients are** and the **outbound endpoint where the destination is**.
- For HTTP(S), set the `Host` header to what the destination expects. Without it many sites answer with the wrong site or an error.
- Use one endpoint per service or port. Two endpoints cannot listen on the same port on one machine.
- If both sites are behind firewalls, point both at a third machine that can accept connections (see the README).

**Operations**

- Run with **Verbose** while setting up and **Warning** afterwards. Use **Debug** only briefly, it is very detailed.
- Set a **log file path** for a service that runs unattended. Exceptions and warnings also go to the Windows Application event log (source `NetTunnel`).
- Set **stale endpoint expiration** if clients leave idle connections open and you want them reclaimed.
- Calendar the certificate expiry date. With the Let's Encrypt manual DNS challenge, renewal is manual.
- Back up `C:\ProgramData\NetTunnel\` (it has your tunnels, users and endpoints).

## 11. Troubleshooting

| Symptom | Likely cause and fix |
| --- | --- |
| The endpoint does not listen and the log mentions the certificate | The PFX path or password is wrong, or the service account cannot read the file. The path is read on the machine that hosts the inbound endpoint. |
| Browser: `ERR_CERT_AUTHORITY_INVALID` | The certificate is self-signed, from an untrusted CA, or from the Let's Encrypt **staging** server. Use a trusted certificate. |
| Browser: name mismatch (`ERR_CERT_COMMON_NAME_INVALID`) | You browsed by an address that is not in the certificate. Use the certificate's host name. |
| Log: `TLS handshake failed ... unknown error occurred while processing the certificate` | The client rejected the certificate. Same causes as the two rows above. |
| Log: `TLS handshake ... ended by the client` | A browser opened and dropped a speculative connection. Harmless. |
| Works with HTTP but fails with HTTPS to the destination | Name mismatch or untrusted certificate at the destination. Use the host name as the termination address, or trust its CA. |
| HTTP/2 clients get an error but HTTP/1.1 works | The destination does not support HTTP/2. The log says the handshake with the destination failed because of it. Turn off **Offer HTTP/2** on the endpoint, or enable HTTP/2 on the destination. |
| Log: `The protected value for ... could not be decrypted` | The configuration file was written on another machine or with another key file. Enter the plain value in the file or in the UI and save. See [Secrets in the files](#secrets-in-the-files). |
| Header rules do nothing | The traffic type is Raw, the rule is disabled, the type is on the wrong end (Request rules act at the inbound end, Response rules at the outbound end), the verb does not match, or the stream switched to passthrough (WebSocket/upgrade). |
| The site loads but images, links or logins go to the backend's name | The backend writes absolute URLs or redirects with its own host name. Header rules cannot rewrite them. Fix at the backend, or use a path that does not depend on the host name. |
| Port already in use | Another program, or another endpoint, is already listening there. Change the listen port. |
| Tunnel stays disconnected | Check the management port is reachable from the outbound side, the address and credentials are right, and the log for the exact error. |
| Unexpected `ConnectionRefused` for an endpoint | The destination is not listening, or the outbound service cannot reach it. Test from the machine that hosts the outbound endpoint. |

## 12. Known limitations

- HTTP/1.x and HTTP/2 are supported, HTTP/3 (QUIC) is not: tunnels carry TCP and HTTP/3 is UDP. Clients fall back to HTTP/2 or HTTP/1.1. WebSockets and other upgrades are forwarded but not inspected.
- HTTP/2 is relayed, not translated: a client that uses HTTP/2 needs a destination that speaks HTTP/2 (see [HTTP/2](#http2)). It is only available with an HTTPS inbound endpoint.
- Header rules cannot change bodies, use variables, or match on values. They are applied on every message in order.
- A response to a `HEAD` request carries a length but no body. NetTunnel cannot tell, so the rest of that connection is passed through unmodified.
- One certificate per endpoint, no client certificates (mutual TLS), no per-name certificate selection.
