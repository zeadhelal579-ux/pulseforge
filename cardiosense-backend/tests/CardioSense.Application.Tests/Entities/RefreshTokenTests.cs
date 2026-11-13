// =============================================================================
// File           : RefreshTokenTests.cs
// Layer          : Tests / Entities
// Purpose        : Verifies token reuse detection: theft is told apart from a harmless double request.
// Logic          : A browser can send the same refresh cookie twice within moments (two tabs, React StrictMode). A
//                  revoked token used inside the leeway is therefore accepted; the same token used after the leeway
//                  is a replay (probably stolen) and must be treated as such.
// Implementation : Plain xUnit facts with a 10 second leeway; no database.
// =============================================================================
using CardioSense.Application.Tests.Support;
using CardioSense.Domain.Entities;

namespace CardioSense.Application.Tests.Entities;

/// <summary>Tests for the RefreshToken entity.</summary>
public sealed class RefreshTokenTests
{
    /// <summary>The instant used as "now".</summary>
    private static readonly DateTimeOffset Now = ApplicationTestHost.Start;

    /// <summary>The leeway used by the refresh handler.</summary>
    private static readonly TimeSpan Leeway = TimeSpan.FromSeconds(10);

    /// <summary>Creates a token that expires in seven days.</summary>
    private static RefreshToken NewToken() => RefreshToken.Create(Guid.NewGuid(), "hash", Now.AddDays(7), Now);

    /// <summary>A fresh token is usable and is not a replay.</summary>
    [Fact]
    public void A_fresh_token_is_usable()
    {
        var token = NewToken();

        Assert.True(token.IsUsable(Now.AddMinutes(1), Leeway));
        Assert.False(token.IsReplay(Now.AddMinutes(1), Leeway));
    }

    /// <summary>A token revoked a moment ago is still accepted (concurrent refresh requests).</summary>
    [Fact]
    public void A_just_revoked_token_is_still_usable_inside_the_leeway()
    {
        var token = NewToken();
        token.Revoke(Now);

        Assert.True(token.IsUsable(Now.AddSeconds(5), Leeway));
        Assert.False(token.IsReplay(Now.AddSeconds(5), Leeway));
    }

    /// <summary>A token revoked longer ago than the leeway is unusable and counts as a replay.</summary>
    [Fact]
    public void A_revoked_token_used_after_the_leeway_is_a_replay()
    {
        var token = NewToken();
        token.Revoke(Now);

        Assert.False(token.IsUsable(Now.AddSeconds(11), Leeway));
        Assert.True(token.IsReplay(Now.AddSeconds(11), Leeway));
    }

    /// <summary>An expired token is unusable.</summary>
    [Fact]
    public void An_expired_token_is_unusable()
    {
        var token = NewToken();

        Assert.False(token.IsUsable(Now.AddDays(8), Leeway));
    }

    /// <summary>Revoking twice keeps the first revocation time.</summary>
    [Fact]
    public void Revoking_twice_keeps_the_first_time()
    {
        var token = NewToken();
        token.Revoke(Now);
        token.Revoke(Now.AddHours(1));

        Assert.Equal(Now, token.RevokedAt);
    }
}
