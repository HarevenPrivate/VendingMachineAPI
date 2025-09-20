namespace VendingMachineAPI.Models;

// Base
public abstract record VendingEvent();

// Events
public sealed record KeyPressEvent(string Key) : VendingEvent;
//public sealed record InsertMoneyEvent(decimal Amount) : VendingEvent;
public sealed record OkEvent() : VendingEvent;
public sealed record CancelEvent() : VendingEvent;

