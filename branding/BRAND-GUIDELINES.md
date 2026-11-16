# MSF Lang visual identity

The supplied `source/msf-lang-brand-board.png` is the official reference for Mustafa Software Factory — MSF Lang. It is a 1536 × 1024 RGB raster image with an opaque, graduated background. The original bytes are preserved. Existing files under `concepts/` are historical proposals, not the adopted identity.

The M monogram, code brackets, hexagonal frame and orbital strokes suggest language engineering, structure and motion. This is an interpretation of the artwork, not a claim about language performance.

## Palette sampled from the source

The artwork contains gradients rather than flat color tokens. These exact source pixel samples are representative anchors, not claims that every pixel has these colors. Coordinates are zero-based in the original image.

| Role | Hex | Source coordinate |
| --- | --- | --- |
| Deep navy | `#050B30` | 768,30 |
| Cyan | `#01EBF8` | 600,220 |
| Violet | `#4F03B6` | 974,290 |
| Light lettering | `#F0F8FD` | 720,260 |

## Assets and limitations

- `logo/msf-lang-logo.png`: primary symbol and wordmark, cropped without resizing or recoloring.
- `logo/msf-lang-logo-dark.png`: identical approved dark presentation.
- `logo/msf-lang-symbol.png`: independent symbol crop.
- `icons/icon-{16,32,48,192,512}.png` and `icons/favicon.ico`: proportionally scaled symbol with navy padding. The 512 export is derived from a 630 × 510 source crop, not vector artwork.
- `logo/msf-lang-logo-light.png` and `logo/msf-lang-logo-monochrome.png`: pending. The reference contains neither treatment; converting its gradients would invent a new variant. On light pages, display the original dark rectangular artwork with its background intact.
- `logo/msf-lang-logo.svg` and `icons/favicon.svg`: pending accurate vector reconstruction from approved editable artwork. No empty SVGs, approximate paths, or embedded bitmap substitutes are supplied.
- Transparency: unavailable in the original; background is retained to preserve glow and edge detail.

## Usage

Preserve aspect ratio, artwork colors and background. Leave at least 10% of the displayed width as external clear space. Recommended minimum wordmark width is 240 CSS pixels; use 32 pixels or larger for the standalone symbol when possible. At 16 pixels, the code brackets and orbital details lose definition; use the icon only for favicon recognition, not as readable typography.

For GitHub README use the relative PNG path already integrated at 360 pixels. Documentation and future websites can reuse the primary lockup in headers and symbol in compact navigation. CLI documentation can display the image in HTML/Markdown; terminal output should retain plain text `MSF Lang` rather than assuming image support. Do not imply that the promotional wording on the original board is a measured performance guarantee.

Do not stretch, rotate, recolor, remove the M or code brackets, apply additional effects, substitute emoji, or reuse historical concept artwork as the official mark. Do not place the wordmark directly on a different background without an approved transparent source.

## Reproduction and verification

Run `powershell -NoProfile -File branding/export-assets.ps1` from the repository to regenerate the crops and icons using Windows System.Drawing; no project dependencies are added. The source copy must already exist.

PNG decoding, exact icon dimensions, all three ICO frames and byte-for-byte source preservation were checked. The logo crop was visually inspected. README retains its original sections and uses an existing relative image path. Interactive editor Markdown Preview was not available during this change.
