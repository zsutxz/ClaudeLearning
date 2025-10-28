#!/usr/bin/env python3
"""
Minimal Hello World example for a Claude Agent SDK coding agent.
"""
import asyncio
import os
from typing import Dict, Any

# Import required modules
from claude_agent_sdk import query
from claude_agent_sdk.types import ClaudeAgentOptions
from dotenv import load_dotenv

async def main() -> None:
    """Main function to run the agent."""
    # Load environment variables from .env file
    load_dotenv()
    
    # Ensure the API key is set in environment variables
    if "ANTHROPIC_API_KEY" not in os.environ:
        print("Error: ANTHROPIC_API_KEY environment variable not set.")
        print("Please set it in your .env file.")
        return
    
    # Create a simple message
    message = "Hello, what can you help me with regarding coding?"
    
    # Create options for the query
    options = ClaudeAgentOptions(
        cwd=os.getcwd()
    )
    
    # Send the query and get the response
    try:
        # The query function returns an async iterator
        async for response in query(prompt=message, options=options):
            # Print the response content
            if hasattr(response, 'content'):
                print(f"Agent response: {response.content}")
            else:
                print(f"Received message: {response}")
    except Exception as e:
        print(f"Error making query: {e}")


if __name__ == "__main__":
    # Run the async main function
    asyncio.run(main())