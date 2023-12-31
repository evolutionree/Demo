﻿using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.Services.Services;

namespace UBeat.Crm.CoreApi.Services.Utility
{
    public class UKJSEngineUtils
    {
        private JavaScriptUtilsServices _javaScriptServices;
        private JavaScriptEngineSwitcher.Jint.JintJsEngine engine = null;
        public UKJSEngineUtils(JavaScriptUtilsServices javaScriptServices) {
            _javaScriptServices = javaScriptServices;
            engine = new JavaScriptEngineSwitcher.Jint.JintJsEngine(new JavaScriptEngineSwitcher.Jint.JintSettings() {

                StrictMode = true
            });
            engine.EmbedHostObject("ukservices", javaScriptServices);
        }
        public object Evaluate(string code) {
            return engine.Evaluate(code);
        }
        public T Evaluate<T>(string code) {
            return engine.Evaluate<T>(code);
        }
        public void Execute(string code) {
            engine.Execute(code);
        }

        public void SetHostedObject(string name ,object obj ) {
            engine.EmbedHostObject(name, obj);
        }
        

    }
}
