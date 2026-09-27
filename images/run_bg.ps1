Add-Type -Path "c:\Users\cc\Desktop\baowao\images\BgRemover.cs" -ReferencedAssemblies "System.Drawing.dll"

$items = @(
    @{ in="c:\Users\cc\Desktop\baowao\images\cpu_5700x.png"; out="c:\Users\cc\Desktop\baowao\images\cpu_transparent.png"; thresh=245 },
    @{ in="c:\Users\cc\Desktop\baowao\images\motherboard_b550m.png"; out="c:\Users\cc\Desktop\baowao\images\motherboard_transparent.png"; thresh=245 },
    @{ in="c:\Users\cc\Desktop\baowao\images\case_datablitz.jpg"; out="c:\Users\cc\Desktop\baowao\images\case_transparent.png"; thresh=242 },
    @{ in="c:\Users\cc\Desktop\baowao\images\psu_p650ss.png"; out="c:\Users\cc\Desktop\baowao\images\psu_transparent.png"; thresh=245 },
    @{ in="c:\Users\cc\Desktop\baowao\images\cooler_astrobeat.png"; out="c:\Users\cc\Desktop\baowao\images\cooler_transparent.png"; thresh=242 },
    @{ in="c:\Users\cc\Desktop\baowao\images\gpu_rtx5060_aero.jpg"; out="c:\Users\cc\Desktop\baowao\images\gpu_transparent.png"; thresh=242 }
)

foreach ($item in $items) {
    Write-Host "Processing $($item.in)..."
    [BgRemover]::Process($item.in, $item.out, $item.thresh)
    $fi = Get-Item $item.out
    Write-Host "Generated $($item.out) - size: $($fi.Length) bytes"
}
