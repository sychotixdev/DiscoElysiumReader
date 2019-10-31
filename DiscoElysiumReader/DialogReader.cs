using System;
using System.Linq.Expressions;
using PixelCrushers.DialogueSystem;
using UnityEngine;
using System.Reflection;
using Debug = UnityEngine.Debug;

namespace DiscoElysiumReaderMod
{
    class DialogReader : MonoBehaviour
    {
        public String Text;
        public String ActorName;
        public String ConversantName;

        private void OnEnable()
        {
            var dialogReaderType = typeof(DialogReader);
            Lua.RegisterFunction("PushConversationPosition", null, SymbolExtensions.GetMethodInfo(Expression.Lambda<Action>(Expression.Call(null, dialogReaderType.GetMethod("PushConversationPosition"), Array.Empty<Expression>()), Array.Empty<ParameterExpression>())));
        }

        private void OnDisable()
        {
            if (this.unregisterOnDisable)
            {
                Lua.UnregisterFunction("PushConversationPosition");
            }
        }

        protected void Start()
        {
            Text = "";
            ConversantName = "";
            ActorName = "";
        }
        protected void Update()
        {
            if (DialogueManager.IsConversationActive && DialogueManager.Instance.ConversationModel.HasValidEntry)
            {
                DialogueManager.Instance.currentConversationState.subtitle;

                Text = "ActiveBigDick";
                ActorName = DialogueManager.Instance.ConversationModel.ActorInfo.Name;
                ConversantName = DialogueManager.Instance.ConversationModel.ConversantInfo.Name;
            }
            else
            {
                Text = "InactiveBigDick";
                ConversantName = "";
                ActorName = "";
            }
        }

        protected void OnGUI()
        {
            if (String.IsNullOrEmpty(Text))
                return;
            //GUI.Label(new Rect(10, 10, 100, 20), "Is this working yet lol");
            UIHelper.Begin($"Testing {System.IO.Directory.GetCurrentDirectory()}", 1,1,1000,100,5,100,100);
            System.IO.File.WriteAllText(@"WriteText.txt", "Help, I'm trapped.");
        }
    }
}
