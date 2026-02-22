using Janthus.Model.Entities;
using Janthus.Model.Services;
using Janthus.Game.Audio;

namespace Janthus.Game.Conversation;

public class ConversationRunner
{
    private readonly IGameDataProvider _dataProvider;
    private readonly PlayerCharacter _player;
    private readonly string _playerClassName;
    private readonly ConversationActionExecutor _actionExecutor;

    private Model.Entities.Conversation _currentConversation;
    private ConversationNode _currentNode;
    private List<ConversationResponse> _availableResponses;

    public bool IsActive => _currentConversation != null;
    public string SpeakerName => _currentNode?.SpeakerName ?? string.Empty;
    public string Text => _currentNode?.Text ?? string.Empty;
    public List<ConversationResponse> AvailableResponses => _availableResponses ?? new();
    public bool IsEndNode => _currentNode?.IsEndNode ?? true;

    public Action<string> OnRecruitFollower
    {
        get => _actionExecutor.OnRecruitFollower;
        set => _actionExecutor.OnRecruitFollower = value;
    }

    public Action<string> OnQuestStarted
    {
        get => _actionExecutor.OnQuestStarted;
        set => _actionExecutor.OnQuestStarted = value;
    }

    public Action<string> OnQuestCompleted
    {
        get => _actionExecutor.OnQuestCompleted;
        set => _actionExecutor.OnQuestCompleted = value;
    }

    public ConversationRunner(IGameDataProvider dataProvider, PlayerCharacter player,
                              string playerClassName, AudioManager audioManager)
    {
        _dataProvider = dataProvider;
        _player = player;
        _playerClassName = playerClassName;
        _actionExecutor = new ConversationActionExecutor(dataProvider, player, audioManager);
    }

    public bool TryStartConversation(string npcName)
    {
        var conversation = ConversationManager.FindConversation(
            _dataProvider, npcName, _player, _playerClassName);

        if (conversation == null)
            return false;

        _currentConversation = conversation;
        NavigateToNode(conversation.EntryNodeId);
        return true;
    }

    public bool SelectResponse(int index)
    {
        if (_availableResponses == null || index < 0 || index >= _availableResponses.Count)
            return false;

        var response = _availableResponses[index];

        _actionExecutor.ExecuteActions(response.Actions);

        if (response.NextNodeId == 0)
        {
            EndConversation();
            return false;
        }

        NavigateToNode(response.NextNodeId);
        return true;
    }

    public void EndConversation()
    {
        if (_currentConversation != null && !_currentConversation.IsRepeatable)
        {
            _dataProvider.SetGameFlag($"conv_completed_{_currentConversation.Id}", "true");
        }

        _currentConversation = null;
        _currentNode = null;
        _availableResponses = null;
    }

    private void NavigateToNode(int nodeId)
    {
        _currentNode = _dataProvider.GetConversationNode(nodeId);
        if (_currentNode == null)
        {
            EndConversation();
            return;
        }

        if (_currentNode.IsEndNode)
        {
            _availableResponses = new List<ConversationResponse>();
        }
        else
        {
            var allResponses = _dataProvider.GetResponsesForNode(nodeId);
            _availableResponses = ConversationManager.GetAvailableResponses(
                allResponses, _player, _playerClassName, _dataProvider);
        }
    }
}
