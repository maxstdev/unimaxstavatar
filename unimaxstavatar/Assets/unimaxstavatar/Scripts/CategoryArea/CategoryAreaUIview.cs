using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Maxst.Avatar
{
    public class CategoryAreaUIview : MonoBehaviour
    {
        [SerializeField] private CategorySubject subjectPrefab;
        [SerializeField] private ScrollRect subjectScrollview;
        private List<CategorySubject> subjectList = new List<CategorySubject>();

        public ReactiveProperty<string> categoryWardrobeslot = new ReactiveProperty<string>();
        public ReactiveProperty<ViewType> categoryChangeViewType = new ReactiveProperty<ViewType>();

        public void CreateSubject(string slot, string koname, Sprite defaulticon, Sprite selectIcon, out Action onclick)
        {
            var subject = Instantiate(subjectPrefab, subjectScrollview.content);
            subject.SetSubject(koname, defaulticon, selectIcon);
            subject.OnclickButton = () =>
            {
                SubjectSelectChange(subject);
                categoryWardrobeslot.SetValueAndForceNotify(slot);
                categoryChangeViewType.Value = GetSlotViewType(slot);
            };
            onclick = subject.OnclickButton;

            subjectList.Add(subject);
        }

        public void DeleteAllSubject()
        {
            foreach (var subject in subjectList)
            {
                Destroy(subject.gameObject);
            }
            subjectList.Clear();
        }

        private void SubjectSelectChange(CategorySubject selected)
        {
            foreach (var subject in subjectList)
            {
                subject.isSelect.Value = false;
            }
            selected.isSelect.Value = true;
        }

        private ViewType GetSlotViewType(string slot)
        {
            return slot switch
            {
                "Eyebrows" => ViewType.Face_Eyebrows,
                "Hair" => ViewType.Face_Hair,
                _ => ViewType.Body
            };
        }
    }
}