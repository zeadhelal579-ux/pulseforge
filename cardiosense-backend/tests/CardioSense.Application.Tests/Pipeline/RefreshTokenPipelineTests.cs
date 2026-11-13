// =============================================================================
// File           : RefreshTokenPipelineTests.cs
// Layer          : Tests / Pipeline
// Purpose        : Verifies session renewal: rotation, the concurrent-request leeway and theft detection.
// Logic          : Every refresh replaces the token. A just-replaced token still works for a few seconds (two tabs);
//                  a replaced token used later is treated as stolen and ends every session of that user.
// Implementation : Seeds a patient account with stored refresh tokens (hashes from the stub token service) and
//                  sends RefreshTokenCommand through the pipeline.
// =============================================================================
using CardioSense.Application.Auth.Commands.RefreshToken;
using CardioSense.Application.Common.Errors;
using CardioSense.Application.Common.Models;
using CardioSense.Application.Tests.Support;
using CardioSense.Domain.Entities;
using CardioSense.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CardioSense.Application.Tests.Pipeline;

/// <summary>Tests for refresh-token handling.</summary>
public sealed class RefreshTokenPipelineTests
{
    /// <summary>Seeds a patient account and returns the patient.</summary>
    private static async Task<Patient> SeedAccountAsync(ApplicationTestHost host)
    {
        var patient = Seed.NewPatient();
        host.Identity.Add(new UserAccount(patient.UserId, "ahmed@example.com", UserRole.Patient, "en"), "pw");
        await host.SeedAsync(patient);
        return patient;
    }

    /// <summary>Stores a refresh token for the user, optionally already revoked at the given time.</summary>
    private static RefreshToken StoredToken(
        ApplicationTestHost host, Guid userId, string raw, DateTimeOffset? revokedAt = null)
    {
        var token = RefreshToken.Create(
            userId, host.Tokens.HashRefreshToken(raw), ApplicationTestHost.Start.AddDays(7), ApplicationTestHost.Start);
        if (revokedAt is { } at)
        {
            token.Revoke(at);
        }

        return token;
    }

    /// <summary>A valid token is exchanged for a new one and the old one is revoked.</summary>
    [Fact]
    public async Task A_valid_token_is_rotated()
    {
        await using var host = new ApplicationTestHost();
        var patient = await SeedAccountAsync(host);
        await host.SeedAsync(StoredToken(host, patient.UserId, "old"));

        var result = await host.SendAsync(new RefreshTokenCommand("old"));

        Assert.True(result.IsSuccess);
        Assert.NotEqual("old", result.Value.RawRefreshToken);
        Assert.Equal(2, await host.QueryAsync(db => db.RefreshTokens.CountAsync()));
        var oldHash = host.Tokens.HashRefreshToken("old");
        var old = await host.QueryAsync(db => db.RefreshTokens.SingleAsync(token => token.TokenHash == oldHash));
        Assert.NotNull(old.RevokedAt);
    }

    /// <summary>A token revoked seconds ago (a double request) is still accepted.</summary>
    [Fact]
    public async Task A_token_revoked_inside_the_leeway_is_accepted()
    {
        await using var host = new ApplicationTestHost();
        var patient = await SeedAccountAsync(host);
        var revokedAt = ApplicationTestHost.Start.AddSeconds(-5);
        await host.SeedAsync(StoredToken(host, patient.UserId, "old", revokedAt));

        var result = await host.SendAsync(new RefreshTokenCommand("old"));

        Assert.True(result.IsSuccess);
    }

    /// <summary>A token revoked long ago and presented again is a replay: reject it and end every session.</summary>
    [Fact]
    public async Task A_replayed_token_ends_every_session_of_the_user()
    {
        await using var host = new ApplicationTestHost();
        var patient = await SeedAccountAsync(host);
        var revokedAt = ApplicationTestHost.Start.AddMinutes(-1);
        await host.SeedAsync(
            StoredToken(host, patient.UserId, "stolen", revokedAt),
            StoredToken(host, patient.UserId, "still-active"));

        var result = await host.SendAsync(new RefreshTokenCommand("stolen"));

        Assert.Equal(ApplicationErrors.Auth.InvalidRefreshToken.Code, result.Error.Code);
        var activeHash = host.Tokens.HashRefreshToken("still-active");
        var active = await host.QueryAsync(db => db.RefreshTokens.SingleAsync(token => token.TokenHash == activeHash));
        Assert.NotNull(active.RevokedAt);
    }

    /// <summary>An unknown or missing token is rejected.</summary>
    [Theory]
    [InlineData("never-issued")]
    [InlineData(null)]
    [InlineData("")]
    public async Task An_unknown_or_missing_token_is_rejected(string? raw)
    {
        await using var host = new ApplicationTestHost();

        var result = await host.SendAsync(new RefreshTokenCommand(raw));

        Assert.Equal(ApplicationErrors.Auth.InvalidRefreshToken.Code, result.Error.Code);
    }
}
