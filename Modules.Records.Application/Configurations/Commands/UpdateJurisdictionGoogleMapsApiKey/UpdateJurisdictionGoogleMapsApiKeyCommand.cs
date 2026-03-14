using MediatR;

namespace Modules.Records.Application.Configurations.Commands.UpdateJurisdictionGoogleMapsApiKey;

/// <summary>Saves the Google Maps API key for the current jurisdiction. Null or empty clears the key.</summary>
public sealed record UpdateJurisdictionGoogleMapsApiKeyCommand(string? ApiKey) : IRequest;
