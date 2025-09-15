namespace VendingMachineAPI.Models;

// Base
public abstract record VendingEvent();

// Events
public sealed record KeyPressEvent(string Key) : VendingEvent;
//public sealed record InsertMoneyEvent(decimal Amount) : VendingEvent;
public sealed record OkEvent() : VendingEvent;
public sealed record CancelEvent() : VendingEvent;
//public sealed record ThermostatStatusEvent(bool Working) : VendingEvent;

public enum VendingStateSatus
{
    Ready,
    ProductNotAvailable,
    OperationCancelling,
    ProductSeletedMissingMoey,
}

// State snapshot returned to clients
public sealed class VendingState
{
    public string? Selection { get; set; } = null;
    public decimal Inserted { get; set; }
    public bool ThermostatWorking { get; set; } = true;
    public bool BlockInputs { get; set; } = false;
    public string Display { get; set; } = "Machine Ready";

    public VendingStateSatus Status { get; set; } = VendingStateSatus.Ready;

    public VendingState Clone()
    {
        return new VendingState
        {
            Selection = this.Selection,
            Inserted = this.Inserted,
            ThermostatWorking = this.ThermostatWorking,
            BlockInputs = this.BlockInputs,
            Display = this.Display,
            Status = this.Status,
        };
    }
}
