using BLE.Toolkit.Sample.Load.Models;
using BLE.Toolkit.Sample.Load.Services;
using BLE.Toolkit.Settings;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ReceivedMessageStore>();
builder.Services.AddSingleton<BleLoadNodeService>();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/transmitter/throttling", (BleLoadNodeService node) =>
  Results.Ok(node.GetThrottling()));

app.MapPut("/api/transmitter/throttling", (SetThrottlingRequest request, BleLoadNodeService node) =>
{
  if (!TryParseRatePeriod(request.RatePeriod, out var ratePeriod))
    return Results.BadRequest(new { error = "RatePeriod must be 'second', 'minute', 'hour', or 'day'." });

  try
  {
    node.SetThrottling(request.Enabled, ratePeriod, request.Limit);
    return Results.Ok(node.GetThrottling());
  }
  catch (ArgumentOutOfRangeException ex)
  {
    return Results.BadRequest(new { error = ex.Message });
  }
});

app.MapGet("/api/transmitter/devices", (BleLoadNodeService node) =>
  Results.Ok(node.GetCachedDevices()));

app.MapGet("/api/node/status", (BleLoadNodeService node) => node.GetStatus());

app.MapPost("/api/node/role", async (SetRoleRequest request, BleLoadNodeService node, CancellationToken ct) =>
{
  if (!TryParseRole(request.Role, out var role))
    return Results.BadRequest(new
    {
      error = "Role must be 'central', 'servernotify', or 'receiver'."
    });

  await node.SetRoleAsync(role, ct);
  return Results.Ok(node.GetStatus());
});

app.MapPost("/api/transmitter/send", async (TransmitRequest request, BleLoadNodeService node, CancellationToken ct) =>
{
  try
  {
    await node.EnqueueTransmissionAsync(request.Message, request.Count, ct);
    return Results.Ok(node.GetStatus());
  }
  catch (InvalidOperationException ex)
  {
    return Results.BadRequest(new { error = ex.Message });
  }
  catch (ArgumentException ex)
  {
    return Results.BadRequest(new { error = ex.Message });
  }
});

app.MapGet("/api/receiver/messages", (int? skip, int? take, BleLoadNodeService node) =>
{
  var response = node.GetMessages(skip ?? 0, Math.Clamp(take ?? 100, 1, 1000));
  return Results.Ok(response);
});

app.MapGet("/api/receiver/count", (BleLoadNodeService node) =>
  Results.Ok(new ReceiverCountResponse(node.GetReceivedCount())));

app.MapDelete("/api/receiver/messages", (BleLoadNodeService node) =>
{
  node.ClearReceivedMessages();
  return Results.Ok(new ReceiverCountResponse(0));
});

app.Run();

static bool TryParseRole(string? role, out NodeRole parsed)
{
  parsed = NodeRole.None;
  if (string.IsNullOrWhiteSpace(role))
    return false;

  return role.Trim().ToLowerInvariant() switch
  {
    "central" or "centraltransmitter" or "transmitter" or "0" =>
      Assign(NodeRole.CentralTransmitter, out parsed),
    "servernotify" or "servernotifytransmitter" or "1" =>
      Assign(NodeRole.ServerNotifyTransmitter, out parsed),
    "receiver" or "2" => Assign(NodeRole.Receiver, out parsed),
    _ => false
  };
}

static bool Assign(NodeRole role, out NodeRole parsed)
{
  parsed = role;
  return true;
}

static bool TryParseRatePeriod(string? ratePeriod, out RatePeriod parsed)
{
  parsed = RatePeriod.Second;
  if (string.IsNullOrWhiteSpace(ratePeriod))
    return false;

  switch (ratePeriod.Trim().ToLowerInvariant())
  {
    case "second":
      parsed = RatePeriod.Second;
      return true;
    case "minute":
      parsed = RatePeriod.Minute;
      return true;
    case "hour":
      parsed = RatePeriod.Hour;
      return true;
    case "day":
      parsed = RatePeriod.Day;
      return true;
    default:
      return false;
  }
}
