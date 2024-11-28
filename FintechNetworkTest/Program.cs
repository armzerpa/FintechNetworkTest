using System;
using System.Collections.Generic;
using System.Linq;

public class FintechNetwork
{
    // Insight class to represent financial insights
    public class Insight
    {
        public int InsightId { get; }
        public int AuthorId { get; }
        public DateTime PublishedAt { get; }
        public string Content { get; }

        public Insight(int insightId, int authorId, string content)
        {
            InsightId = insightId;
            AuthorId = authorId;
            Content = content;
            PublishedAt = DateTime.UtcNow;
        }
    }

    // User class to represent network users
    public class User
    {
        public int UserId { get; }
        public string Username { get; }
        public List<Insight> PublishedInsights { get; }
        public HashSet<int> FollowedUserIds { get; }

        public User(int userId, string username)
        {
            UserId = userId;
            Username = username;
            PublishedInsights = new List<Insight>();
            FollowedUserIds = new HashSet<int>();
        }
    }

    // Professional connection class
    public class ProfessionalConnection
    {
        public enum ConnectionStatus
        {
            Pending,
            Connected
        }

        public int UserId { get; }
        public int ProfessionalId { get; }
        public ConnectionStatus Status { get; set; }
        public DateTime ConnectionRequestedAt { get; }

        public ProfessionalConnection(int userId, int professionalId)
        {
            UserId = userId;
            ProfessionalId = professionalId;
            Status = ConnectionStatus.Pending;
            ConnectionRequestedAt = DateTime.UtcNow;
        }
    }

    // Network-level collections
    private Dictionary<int, User> _users;
    private Dictionary<int, Insight> _insights;
    private HashSet<int> _uniqueInsightIds;
    private Dictionary<int, HashSet<ProfessionalConnection>> _professionalConnections;

    // Constructor
    public FintechNetwork()
    {
        _users = new Dictionary<int, User>();
        _insights = new Dictionary<int, Insight>();
        _uniqueInsightIds = new HashSet<int>();
        _professionalConnections = new Dictionary<int, HashSet<ProfessionalConnection>>();
    }

    // Method to register a new user
    public void RegisterUser(int userId, string username)
    {
        if (_users.ContainsKey(userId))
        {
            throw new InvalidOperationException($"User with ID {userId} already exists.");
        }

        _users[userId] = new User(userId, username);
    }

    // Method to share a new financial insight with unique ID constraint
    public Insight ShareInsight(int userId, int insightId, string content = "")
    {
        // Validate user exists
        if (!_users.ContainsKey(userId))
        {
            throw new ArgumentException($"User with ID {userId} does not exist.");
        }

        // Enforce unique insight ID across the entire network
        if (_uniqueInsightIds.Contains(insightId))
        {
            throw new InvalidOperationException($"Insight with ID {insightId} has already been published in the network.");
        }

        // Create new insight
        Insight newInsight = new Insight(insightId, userId, content);

        // Add insight to network tracking
        _uniqueInsightIds.Add(insightId);
        _insights[insightId] = newInsight;

        // Add insight to user's published insights
        _users[userId].PublishedInsights.Add(newInsight);

        return newInsight;
    }

    // Method to follow a user
    public void FollowUser(int followerId, int followedUserId)
    {
        if (!_users.ContainsKey(followerId) || !_users.ContainsKey(followedUserId))
        {
            throw new ArgumentException("One or both users do not exist.");
        }

        _users[followerId].FollowedUserIds.Add(followedUserId);
    }

    // Method to retrieve latest insights for a user's personalized feed
    public List<int> GetLatestInsights(int userId)
    {
        // Validate user exists
        if (!_users.ContainsKey(userId))
        {
            throw new ArgumentException($"User with ID {userId} does not exist.");
        }

        // Collect insights from followed users and the user themselves
        var followedUserIds = _users[userId].FollowedUserIds.ToList();
        followedUserIds.Add(userId); // Include user's own insights

        // Retrieve and sort insights from followed users
        var latestInsights = followedUserIds
            .SelectMany(followedUserId => _users[followedUserId].PublishedInsights)
            .OrderByDescending(i => i.PublishedAt)
            .Take(10)
            .Select(i => i.InsightId)
            .ToList();

        return latestInsights;
    }

    // Method to connect with a professional
    public void ConnectWithProfessional(int userId, int professionalId)
    {
        // Validate both users exist
        if (!_users.ContainsKey(userId) || !_users.ContainsKey(professionalId))
        {
            throw new ArgumentException("One or both users do not exist.");
        }

        // Prevent connecting with self
        if (userId == professionalId)
        {
            throw new InvalidOperationException("Cannot connect with yourself.");
        }

        // Initialize connections for user if not exists
        if (!_professionalConnections.ContainsKey(userId))
        {
            _professionalConnections[userId] = new HashSet<ProfessionalConnection>();
        }

        // Check if connection already exists
        var existingConnection = _professionalConnections[userId]
            .FirstOrDefault(c => c.ProfessionalId == professionalId);

        if (existingConnection != null)
        {
            // If connection exists but is not connected, throw exception
            if (existingConnection.Status != ProfessionalConnection.ConnectionStatus.Connected)
            {
                throw new InvalidOperationException("Connection request already exists.");
            }

            // If already connected, do nothing
            return;
        }

        // Create new professional connection
        var newConnection = new ProfessionalConnection(userId, professionalId);
        _professionalConnections[userId].Add(newConnection);
    }

    // Example usage method
    public static void Main()
    {
        FintechNetwork network = new FintechNetwork();

        // Register users
        network.RegisterUser(1, "Financial_Guru");
        network.RegisterUser(2, "Market_Analyst");
        network.RegisterUser(3, "Tech_Investor");

        // Follow users
        network.FollowUser(3, 1);
        network.FollowUser(3, 2);

        // Share insights
        network.ShareInsight(1, 101, "Blockchain technology trends");
        network.ShareInsight(2, 102, "Market analysis 2024");
        network.ShareInsight(3, 103, "Tech startup investments");
        network.ShareInsight(1, 104, "Cryptocurrency market update");
        network.ShareInsight(2, 105, "Global economic outlook");

        // Connect with professional
        network.ConnectWithProfessional(3, 1);

        // Get latest insights
        var latestInsights = network.GetLatestInsights(3);

        Console.WriteLine("Latest Insights:");
        foreach (int insightId in latestInsights)
        {
            Console.WriteLine($"Insight ID: {insightId}");
        }
    }
}