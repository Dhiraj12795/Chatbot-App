using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddControllers();

var app = builder.Build();

// Use CORS
app.UseCors();

// Serve static files
app.UseStaticFiles();

// Default route to serve chatbot UI
app.MapGet("/", async context =>
{
    var html = @"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Interactive Chatbot</title>
    <style>
        /* Styling for the chatbot UI */
        body {
            margin: 0;
            padding: 0;
            font-family: Arial, sans-serif;
            background: url('https://static.vecteezy.com/system/resources/thumbnails/039/664/315/small_2x/ai-generated-3d-chatbot-gpt-mascot-futuristic-technology-blue-banner-robot-assistant-copy-space-ai-generated-photo.jpg') no-repeat center center fixed;
            background-size: cover;
            color: #333;
            display: flex;
            justify-content: flex-start;
            align-items: center;
            height: 100vh;
            padding-left: 20px;
        }

        .chat-container {
            background: rgba(255, 255, 255, 0.9);
            border-radius: 15px;
            box-shadow: 0 8px 16px rgba(0, 0, 0, 0.2);
            max-width: 400px;
            width: 90%;
            padding: 20px;
            text-align: center;
        }

        .chat-container h1 {
            margin-bottom: 20px;
            font-size: 24px;
            color: #4facfe;
        }

        #chatDisplay {
            height: 200px;
            overflow-y: auto;
            margin-bottom: 15px;
            padding: 10px;
            border: 1px solid #ddd;
            border-radius: 10px;
            background: #f9f9f9;
            text-align: left;
        }

        #chatDisplay p {
            margin: 5px 0;
            padding: 8px 10px;
            border-radius: 10px;
            display: inline-block;
        }

        .user-message {
            background: #4facfe;
            color: white;
            align-self: flex-end;
        }

        .bot-message {
            background: #f1f1f1;
            color: #333;
        }

        .input-group {
            display: flex;
            margin-top: 10px;
            gap: 10px;
        }

        #messageInput {
            flex: 1;
            padding: 10px;
            border: 1px solid #ddd;
            border-radius: 5px;
            font-size: 14px;
        }

        #sendBtn {
            background: #4facfe;
            color: white;
            border: none;
            padding: 10px 20px;
            border-radius: 5px;
            font-size: 14px;
            cursor: pointer;
            transition: background-color 0.3s ease;
        }

        #sendBtn:hover {
            background: #3287e0;
        }
    </style>
</head>
<body>
    <div class='chat-container'>
        <h1>Chatbot</h1>
        <div id='chatDisplay'></div>
        <div class='input-group'>
            <input type='text' id='messageInput' placeholder='Type your message here...' />
            <button id='sendBtn' onclick='sendMessage()'>Send</button>
        </div>
    </div>

    <script>
        function appendMessage(sender, text) {
            const chatDisplay = document.getElementById('chatDisplay');
            const messageElement = document.createElement('p');
            messageElement.classList.add(sender === 'user' ? 'user-message' : 'bot-message');
            messageElement.innerText = text;
            chatDisplay.appendChild(messageElement);
            chatDisplay.scrollTop = chatDisplay.scrollHeight;
        }

        async function sendMessage() {
            const messageInput = document.getElementById('messageInput');
            const userMessage = messageInput.value.trim();

            if (!userMessage) {
                alert('Please enter a message.');
                return;
            }

            appendMessage('user', userMessage);
            messageInput.value = '';

            try {
                const response = await fetch('/api/chatbot/getResponse', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ Message: userMessage }),
                });

                if (response.ok) {
                    const data = await response.json();
                    appendMessage('bot', data.Response || 'The chatbot has no response.');
                } else {
                    appendMessage('bot', 'Error contacting the chatbot.');
                }
            } catch (error) {
                appendMessage('bot', 'Unable to connect to the server.');
            }
        }
    </script>
</body>
</html>";
    await context.Response.WriteAsync(html);
});

// API route for Chatbot
app.MapPost("/api/chatbot/getResponse", async (HttpContext context) =>
{
    var request = await context.Request.ReadFromJsonAsync<ChatRequest>();

    // Check if the message is valid
    if (request == null || string.IsNullOrWhiteSpace(request.Message))
    {
        await context.Response.WriteAsJsonAsync(new ChatResponse { Response = "I didn't understand your message. Can you please rephrase it?" });
        return;
    }

    // Debugging log - check the received message
    Console.WriteLine($"Received message: {request.Message}");

    string responseMessage = GenerateResponse(request.Message);
    await context.Response.WriteAsJsonAsync(new ChatResponse { Response = responseMessage });
});

app.Run();

// Logic for chatbot response generation
static string GenerateResponse(string userMessage)
{
    userMessage = userMessage.ToLower();

    // Basic debugging
    Console.WriteLine($"Processing message: {userMessage}");

    if (userMessage.Contains("hello") || userMessage.Contains("hi"))
        return "Hello! How can I assist you today?";

    if (userMessage.Contains("your name"))
        return "I'm your friendly chatbot! You can call me ChatBot.";

    if (userMessage.Contains("weather"))
        return "I'm not connected to a weather API right now, but it's always a good day to chat!";

    if (userMessage.Contains("bye"))
        return "Goodbye! Have a wonderful day!";

    // Default response for unrecognized inputs
    return "I'm not sure how to respond to that. Can you tell me more?";
}

// Models
public class ChatRequest
{
    public string? Message { get; set; }
}

public class ChatResponse
{
    public string? Response { get; set; }
}
