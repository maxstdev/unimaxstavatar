using System.Collections.Generic;
using System.Linq;
using UMA;
using UniRx;

namespace Maxst.Avatar
{
    public enum NakedStatus
    {
        top,
        bottom
    }

    public class NakedCheker
    {
        private Dictionary<NakedStatus, List<MaxstWardrobeSlot>> nakedDataDictionary = new();
        public ReactiveCollection<UMATextRecipe> slotListProperty = new();

        public NakedCheker()
        {
            slotListProperty
                .ObserveAdd()
                .Subscribe(slot =>
                {
                    AddList(slot.Value);
                });

            slotListProperty
                .ObserveRemove()
                .Subscribe(slot =>
                {
                    RemoveList(slot.Value);
                });

            nakedDataDictionary.Add(NakedStatus.top, new List<MaxstWardrobeSlot>() 
            { 
                MaxstWardrobeSlot.Chest, 
                MaxstWardrobeSlot.Set,
                MaxstWardrobeSlot.Undertop
            });
            nakedDataDictionary.Add(NakedStatus.bottom, new List<MaxstWardrobeSlot>() 
            {
                MaxstWardrobeSlot.Legs, 
                MaxstWardrobeSlot.Set,
                MaxstWardrobeSlot.Underbottom
            });
        }

        ~NakedCheker()
        {
            slotListProperty.Dispose();
        }

        public List<UMATextRecipe> GetNakedList(List<UMATextRecipe> list)
        {
            foreach (var item in list)
            {
                slotListProperty.Add(item);
            }

            return slotListProperty.ToList();
        }

        private void AddList(UMATextRecipe filter)
        {
            foreach (var slot in slotListProperty)
            {
                if (nakedDataDictionary.Any(data => 
                        data.Value.Contains(filter.MaxstWardrobeSlot) && data.Value.Contains(slot.MaxstWardrobeSlot)))
                {
                    slot.nakedstatus = SlotStatus.Hide; 
                }
            }

            filter.nakedstatus = SlotStatus.Show;

            foreach (var slot in slotListProperty)
            {
                if (slot.MaxstWardrobeSlot.Equals(MaxstWardrobeSlot.Undertop) && IsNakedSlot(NakedStatus.top))
                {
                    slot.nakedstatus = SlotStatus.Show;
                }

                if (slot.MaxstWardrobeSlot.Equals(MaxstWardrobeSlot.Underbottom) && IsNakedSlot(NakedStatus.bottom))
                {
                    slot.nakedstatus = SlotStatus.Show;
                }
            }
        }

        private void RemoveList(UMATextRecipe filter)
        {
            
        }
        
        public bool IsNakedSlot(NakedStatus part)
        {
            foreach(var slot in slotListProperty)
            {
                if (nakedDataDictionary[part].Contains(slot.MaxstWardrobeSlot))
                {
                    if(slot.nakedstatus == SlotStatus.Show)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
