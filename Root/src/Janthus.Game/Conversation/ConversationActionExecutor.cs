using Janthus.Model.Entities;
using Janthus.Model.Enums;
using Janthus.Model.Services;

namespace Janthus.Game.Conversation;

public class ConversationActionExecutor
{
    private readonly IGameDataProvider _dataProvider;
    private readonly PlayerCharacter _player;

    public Action<string> OnRecruitFollower { get; set; }

    public ConversationActionExecutor(IGameDataProvider dataProvider, PlayerCharacter player)
    {
        _dataProvider = dataProvider;
        _player = player;
    }

    public void ExecuteActions(List<ConversationAction> actions)
    {
        foreach (var action in actions)
        {
            ExecuteAction(action);
        }
    }

    private void ExecuteAction(ConversationAction action)
    {
        switch (action.ActionType)
        {
            case ConversationActionType.SetFlag:
                _dataProvider.SetGameFlag(action.Value, "true");
                break;

            case ConversationActionType.ClearFlag:
                _dataProvider.ClearGameFlag(action.Value);
                break;

            case ConversationActionType.GiveGold:
                if (decimal.TryParse(action.Value, out var giveAmount))
                    _player.Gold += giveAmount;
                break;

            case ConversationActionType.TakeGold:
                if (decimal.TryParse(action.Value, out var takeAmount))
                    _player.Gold = Math.Max(0, _player.Gold - takeAmount);
                break;

            case ConversationActionType.GiveItem:
                var giveItem = _dataProvider.GetItemByName(action.Value);
                if (giveItem != null)
                {
                    var existing = _player.Inventory.Find(i => i.Item.Id == giveItem.Id);
                    if (existing != null)
                        existing.Quantity++;
                    else
                        _player.Inventory.Add(new InventoryItem(giveItem));
                }
                break;

            case ConversationActionType.TakeItem:
                var takeItem = _player.Inventory.Find(i => i.Item.Name == action.Value);
                if (takeItem != null)
                {
                    takeItem.Quantity--;
                    if (takeItem.Quantity <= 0)
                        _player.Inventory.Remove(takeItem);
                }
                break;

            case ConversationActionType.SetDisposition:
                if (Enum.TryParse<DispositionType>(action.Value, true, out var disposition))
                    _player.Alignment.Disposition = disposition;
                break;

            case ConversationActionType.SetLawfulness:
                if (Enum.TryParse<LawfulnessType>(action.Value, true, out var lawfulness))
                    _player.Alignment.Lawfulness = lawfulness;
                break;

            case ConversationActionType.StartQuest:
                _dataProvider.SetGameFlag($"quest_active_{action.Value}", "true");
                break;

            case ConversationActionType.CompleteQuest:
                _dataProvider.ClearGameFlag($"quest_active_{action.Value}");
                _dataProvider.SetGameFlag($"quest_done_{action.Value}", "true");
                break;

            case ConversationActionType.GiveExperience:
                System.Console.WriteLine($"[Action] Give {action.Value} XP");
                break;

            case ConversationActionType.RecruitFollower:
                OnRecruitFollower?.Invoke(action.Value);
                break;

            case ConversationActionType.None:
            default:
                break;
        }
    }
}
