window.glitchChat = {
    connection: null,

    // Connect to the existing SignalR ChatHub
    async connect(dotNetReference) {

        this.connection = new signalR.HubConnectionBuilder()
            .withUrl("/chathub")
            .withAutomaticReconnect()
            .build();

        // Receive messages from the server
        this.connection.on("ReceiveMessage", async (message) => {
            await dotNetReference.invokeMethodAsync(
                "ReceiveMessage",
                message.id,
                message.senderProfileID,
                message.receiverProfileID,
                message.text,
                message.sentAt
            );
        });

        // Notify Blazor when the connection is lost
        this.connection.onreconnecting(async () => {
            await dotNetReference.invokeMethodAsync(
                "ConnectionStatusChanged",
                false
            );
        });

        // Notify Blazor when the connection is restored
        // Reload chat history after SignalR reconnects
        // Notify Blazor when the connection is restored
        this.connection.onreconnected(async () => {
            console.log("SignalR: Connection restored");

            // Retry the Blazor callback until it succeeds
            for (let attempt = 1; attempt <= 20; attempt++) {
                // Stop retrying if SignalR disconnects again
                if (this.connection.state !== "Connected") {
                    return;
                }

                try {
                    await dotNetReference.invokeMethodAsync(
                        "ConnectionStatusChanged",
                        true
                    );

                    console.log("SignalR: Blazor callback completed");
                    return;
                }
                catch (error) {
                    console.warn(
                        `SignalR: Blazor callback failed (attempt ${attempt})`,
                        error
                    );

                    // Wait before retrying
                    await new Promise(resolve =>
                        setTimeout(resolve, 1000)
                    );
                }
            }

            console.error("SignalR: Could not notify Blazor");
        });

        await this.connection.start();
    },

    // Send a message through ChatHub
    async sendMessage(receiverProfileId, text) {
        await this.connection.invoke(
            "SendMessage",
            receiverProfileId,
            text
        );
    },

    // Load messages between the two matched profiles
    async getHistory(otherProfileId) {
        return await this.connection.invoke(
            "GetChatHistoryAsync",
            otherProfileId
        );
    },

    // Close the connection when leaving the chat
    async disconnect() {
        if (this.connection) {
            await this.connection.stop();
            this.connection = null;
        }
    }
};