﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Olive.Entities.Data;

namespace Olive.Mvc.Testing
{
    public static class TestingExtensions
    {
        public static IApplicationBuilder UseWebTest<TDatabaseManager>(this IApplicationBuilder app,
            Func<Task> createReferenceData,
            Action<IDevCommandsConfig> config = null)
            where TDatabaseManager : DatabaseManager, new()
        {
            if (!WebTestConfig.IsActive()) return app;

            WebTestConfig.ReferenceDataCreator = createReferenceData;

            var settings = new WebTestConfig
            {
                DatabaseManager = new TDatabaseManager()
            };

            config?.Invoke(settings);

            if (settings.AddDefaultHandlers)
                settings.AddDatabaseManager()
                    .AddTimeInjector()
                    .AddSqlProfile()
                    .AddClearDatabaseCache();

            app.UseMiddleware<WebTestMiddleware>();

            Controller.OnFirstRequest += () => Task.Factory.RunSync(() => TempDatabase.Create(settings));

            return app;
        }
    }
}

namespace Olive.Mvc
{
    using Olive.Mvc.Testing;

    public static class TestingExtensions
    {
        public static HtmlString WebTestWidget(this IHtmlHelper html)
        {
            if (!WebTestConfig.IsActive()) return null;

            if (Context.Current.Request().IsAjaxCall()) return null;

            if (WebTestConfig.IsAutoExecMode)
                html.RunJavascript("page.skipNewWindows();");

            return new HtmlString(WebTestManager.GetWebTestWidgetHtml());
        }
    }
}