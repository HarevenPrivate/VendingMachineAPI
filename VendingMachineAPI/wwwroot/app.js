let connectionPanel;
let connectionMoney;

window.addEventListener("DOMContentLoaded", async () => {
    connectionPanel = new signalR.HubConnectionBuilder()
        .withUrl("/hubs/panel")
        .configureLogging(signalR.LogLevel.Information)
        .build();

    connectionPanel.on("PanelUpdate", (update) => {
        const lcd = document.getElementById("lcd");
        lcd.textContent = update.message;

        const debug = document.getElementById("debug");
        debug.textContent = JSON.stringify(update, null, 2);
    });

    connectionMoney = new signalR.HubConnectionBuilder()
        .withUrl("/hubs/money")
        .configureLogging(signalR.LogLevel.Information)
        .build();

    connectionMoney.on("MoneyUpdate", (update) => {
        const moneyPanel = document.getElementById("money");
        moneyPanel.textContent = update.message;

    });

    try {
        await connectionPanel.start();
        await connectionMoney.start();
        console.log("Connected to vending hub");
    } catch (err) {
        console.error("Error connecting to hub:", err);
    }
});

async function pressKey(key) {
    console.log("Sending key:", key);
    await fetch("/api/keyboard/press", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ key })
    });
}

async function pressOk() {
    await pressKey("OK");
}

async function pressCancel() {
    await pressKey("CANCEL");
}

async function insertMoney(amount) {
    console.log("Inserting money:", amount);
    await fetch("/api/money/insert", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ amount })
    });
}

async function ThermostatStatus(working) {
    await fetch("/api/thermostat/status", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ working })
    });
}
