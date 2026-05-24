namespace Modules.Records.Application.Common;

/// <summary>
/// Marks a command that legitimately carries a large binary payload (e.g. an
/// image upload) and so must be exempt from the demo account's per-write size
/// cap, which is calibrated for text. Such commands enforce their own size
/// limit (content length + type) in their validator. The demo creation-rate cap
/// still applies if the command is also <see cref="IRateLimitedCommand"/>.
/// </summary>
public interface IExemptFromDemoWriteSizeLimit;
