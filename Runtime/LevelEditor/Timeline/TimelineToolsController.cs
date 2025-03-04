using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Telegraphist.LevelEditor.Timeline
{
    public enum TimelineToolType
    {
        None,
        Select,
        Edit
    }
    
    [Serializable]
    public class TimelineToolData
    {
        [SerializeField] private TimelineToolType type;
        [SerializeField] private GameObject tool;
        
        public TimelineToolType Type => type;
        public ITimelineTool Tool => tool ? tool.GetComponent<ITimelineTool>() : new NoTimelineTool();
    }
    
    public class TimelineToolsController : MonoBehaviour
    {
        [SerializeField] private TimelineToolData[] tools;
        [SerializeField] private TimelineToolType defaultToolType = TimelineToolType.None;
        
        private Dictionary<TimelineToolType, ITimelineTool> toolsMap = new();
        private ReactiveProperty<TimelineToolType> activeToolType = new(TimelineToolType.None);
        
        public IObservable<TimelineToolType> OnActiveToolChange => activeToolType;
        public TimelineToolType ActiveToolType => activeToolType.Value;
        public ITimelineTool ActiveTool => toolsMap[ActiveToolType];

        private void Awake()
        {
            OnActiveToolChange.DistinctUntilChanged().Pairwise().Subscribe(x =>
            {
                toolsMap[x.Previous].OnToolDeactivate();
                toolsMap[x.Current].OnToolActivate();
            });
            
            toolsMap.Add(TimelineToolType.None, new NoTimelineTool());
            foreach (var toolData in tools)
            {
                toolsMap.Add(toolData.Type, toolData.Tool);
            }
            
            activeToolType.Value = defaultToolType;
        }

        private void Start()
        {
            LevelEditorInputReceiver.Current.OnToolTypeChanged.Subscribe(SetActiveTool).AddTo(this);
        }

        public void SetActiveTool(TimelineToolType toolType)
        {
            activeToolType.Value = toolType;
        }
    }
}