using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class ResultArea : MonoBehaviour
{
    [SerializeField] private Button againBtn;
    [SerializeField] private Button startBtn;
    
    public Action OnClickAgain;
    public Action OnClickStart;

    void Start()
    {
        againBtn.
           OnClickAsObservable()
           .Subscribe(_ =>
           {
               OnClickAgain?.Invoke();
           })
           .AddTo(this);

        startBtn.
           OnClickAsObservable()
           .Subscribe(_ =>
           {
               OnClickStart?.Invoke();
           })
           .AddTo(this);
    }

    void Update()
    {
        
    }
}
