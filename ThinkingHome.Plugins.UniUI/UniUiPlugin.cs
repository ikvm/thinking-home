﻿using System;
using System.Diagnostics;
using System.Linq;
using ECM7.Migrator.Framework;
using NHibernate.Linq;
using NHibernate.Mapping.ByCode;
using ThinkingHome.Core.Plugins;
using ThinkingHome.Plugins.Listener.Api;
using ThinkingHome.Plugins.Listener.Attributes;
using ThinkingHome.Plugins.UniUI.Model;
using ThinkingHome.Plugins.WebUI.Attributes;

[assembly: MigrationAssembly("ThinkingHome.Plugins.UniUI")]

namespace ThinkingHome.Plugins.UniUI
{
	[Plugin]

	[AppSection("Dashboard list", SectionType.System, "/webapp/uniui/settings/dashboard-list.js", "ThinkingHome.Plugins.UniUI.Resources.Settings.dashboard-list.js")]
	[JavaScriptResource("/webapp/uniui/settings/dashboard-list-view.js", "ThinkingHome.Plugins.UniUI.Resources.Settings.dashboard-list-view.js")]
	[JavaScriptResource("/webapp/uniui/settings/dashboard-list-model.js", "ThinkingHome.Plugins.UniUI.Resources.Settings.dashboard-list-model.js")]
	[HttpResource("/webapp/uniui/settings/dashboard-list.tpl", "ThinkingHome.Plugins.UniUI.Resources.Settings.dashboard-list.tpl")]
	[HttpResource("/webapp/uniui/settings/dashboard-list-item.tpl", "ThinkingHome.Plugins.UniUI.Resources.Settings.dashboard-list-item.tpl")]

	[JavaScriptResource("/webapp/uniui/settings/dashboard-info.js", "ThinkingHome.Plugins.UniUI.Resources.Settings.dashboard-info.js")]
	[JavaScriptResource("/webapp/uniui/settings/dashboard-info-view.js", "ThinkingHome.Plugins.UniUI.Resources.Settings.dashboard-info-view.js")]
	[JavaScriptResource("/webapp/uniui/settings/dashboard-info-model.js", "ThinkingHome.Plugins.UniUI.Resources.Settings.dashboard-info-model.js")]
	[HttpResource("/webapp/uniui/settings/dashboard-info.tpl", "ThinkingHome.Plugins.UniUI.Resources.Settings.dashboard-info.tpl")]
	[HttpResource("/webapp/uniui/settings/dashboard-info-widget.tpl", "ThinkingHome.Plugins.UniUI.Resources.Settings.dashboard-info-widget.tpl")]

	public class UniUiPlugin : PluginBase
	{
		public override void InitDbModel(ModelMapper mapper)
		{
			mapper.Class<Dashboard>(cfg => cfg.Table("UniUI_Dashboard"));
			mapper.Class<Widget>(cfg => cfg.Table("UniUI_Widget"));
			mapper.Class<WidgetParameter>(cfg => cfg.Table("UniUI_WidgetParameter"));
		}

		#region dashboard api

		[HttpCommand("/api/uniui/dashboard/list")]
		public object GetDashboardList(HttpRequestParams request)
		{
			using (var session = Context.OpenSession())
			{
				var list = session.Query<Dashboard>().ToList();

				var model = list
					.Select(x => new
					{
						id = x.Id,
						title = x.Title,
						sortOrder = x.SortOrder
					})
					.ToList();

				return model;
			}
		}

		[HttpCommand("/api/uniui/dashboard/add")]
		public object AddDashboard(HttpRequestParams request)
		{
			string title = request.GetRequiredString("title");

			using (var session = Context.OpenSession())
			{
				var dashboard = new Dashboard
				{
					Id = Guid.NewGuid(),
					Title = title,
					SortOrder = int.MaxValue
				};

				session.Save(dashboard);
				session.Flush();

				return dashboard.Id;
			}
		}

		[HttpCommand("/api/uniui/dashboard/rename")]
		public object RenameDashboard(HttpRequestParams request)
		{
			Guid id = request.GetRequiredGuid("id");
			string title = request.GetRequiredString("title");

			using (var session = Context.OpenSession())
			{
				var dashboard = session.Get<Dashboard>(id);

				if (dashboard != null)
				{
					dashboard.Title = title;

					session.Save(dashboard);
					session.Flush();
				}
			}

			return null;
		}

		[HttpCommand("/api/uniui/dashboard/delete")]
		public object DeleteDashboard(HttpRequestParams request)
		{
			Guid id = request.GetRequiredGuid("id");

			using (var session = Context.OpenSession())
			{
				var dashboard = session.Get<Dashboard>(id);

				if (dashboard != null)
				{
					session.Delete(dashboard);
					session.Flush();
				}
			}

			return null;
		}

		[HttpCommand("/api/uniui/dashboard/move")]
		public object MoveDashboard(HttpRequestParams request)
		{
			Guid id = request.GetRequiredGuid("id");
			bool moveUp = request.GetRequiredBool("up");

			using (var session = Context.OpenSession())
			{
				var list = session.Query<Dashboard>().OrderBy(d => d.SortOrder).ToList();
				var index = list.FindIndex(d => d.Id == id);

				if (index >= 0)
				{
					var otherIndex = moveUp ? index - 1 : index + 1;

					if (otherIndex >= 0 && otherIndex < list.Count)
					{
						var tmp = list[otherIndex];
						list[otherIndex] = list[index];
						list[index] = tmp;

						for (int i = 0; i < list.Count; i++)
						{
							list[i].SortOrder = i;
							session.Save(list[i]);
						}

						session.Flush();
					}
				}
			}

			return null;
		}

		#endregion

		#region widget api

		[HttpCommand("/api/uniui/dashboard/info")]
		public object GetDashboardInfo(HttpRequestParams request)
		{
			Guid dashboardId = request.GetRequiredGuid("id");

			using (var session = Context.OpenSession())
			{
				var dashboard = session.Get<Dashboard>(dashboardId);
				var list = session.Query<Widget>().Where(w => w.Dashboard.Id == dashboardId);

				var info = new
				{
					id = dashboard.Id,
					title = dashboard.Title,
					sortOrder = dashboard.SortOrder
				};

				var widgets = list
					.Select(w => new
					{
						id = w.Id,
						type = w.TypeAlias,
						sortOrder = w.SortOrder
					})
					.ToList();

				return new { info, widgets };
			}
		}

		#endregion
	}
}