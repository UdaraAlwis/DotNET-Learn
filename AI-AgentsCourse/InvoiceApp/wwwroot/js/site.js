// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Wait for Alpine.js to be available
document.addEventListener('DOMContentLoaded', function () {
    // Make sure Alpine is loaded
    if (typeof Alpine !== 'undefined') {
        initializeAlpine();
    } else {
        // If Alpine isn't loaded yet, wait for it
        document.addEventListener('alpine:init', initializeAlpine);
    }
});

function initializeAlpine() {
    Alpine.data('chatbot', () => ({
        chatOpen: false,
        messages: [],
        newMessage: '',
        isLoading: false,

        init() {
            // Add an initial welcome message
            this.messages.push({
                role: 'assistant',
                contents: [{
                    "$type": "text",
                    text: 'Hello! How can I help you with your invoices today?'
                }]
            });

            // Scroll to the bottom of messages when they change
            this.$watch('messages', () => {
                this.$nextTick(() => {
                    const chatMessages = document.getElementById('chat-messages');
                    if (chatMessages) {
                        chatMessages.scrollTop = chatMessages.scrollHeight;
                    }
                });
            });
        },

        toggleChat() {
            this.chatOpen = !this.chatOpen;
        },

        async sendMessage() {
            if (!this.newMessage.trim() || this.isLoading) return;

            // Add user message to the chat
            const userMessage = {
                role: 'user',
                contents: [{
                    "$type": "text",
                    text: this.newMessage.trim()
                }]
            };
            this.messages.push(userMessage);
            this.newMessage = ''; // Clear input field
            this.isLoading = true;

            try {
                // Send the entire messages array to the API
                const response = await fetch('http://localhost:5001/chat', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify(this.messages)
                });

                if (!response.ok) {
                    throw new Error(`HTTP error! Status: ${response.status}`);
                }

                // Get the assistant's response
                const assistantMessages = await response.json();

                // Add the assistant's response to the chat
                this.messages.push(...assistantMessages);
            } catch (error) {
                console.error('Error sending message:', error);

                // Add an error message to the chat
                this.messages.push({
                    role: 'assistant',
                    contents: [{
                        "$type": "text",
                        text: 'Sorry, there was an error processing your request. Please try again later.'
                    }]
                });
            } finally {
                this.isLoading = false;
            }
        }
    }));
}
