/*
This file is part of Keepass2Android, Copyright 2013 Philipp Crocoll. This file is based on Keepassdroid, Copyright Brian Pellin.

  Keepass2Android is free software: you can redistribute it and/or modify
  it under the terms of the GNU General Public License as published by
  the Free Software Foundation, either version 2 of the License, or
  (at your option) any later version.

  Keepass2Android is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU General Public License for more details.

  You should have received a copy of the GNU General Public License
  along with Keepass2Android.  If not, see <http://www.gnu.org/licenses/>.
  */

using System;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using KeePassLib;

namespace keepass2android.view
{
	
	public sealed class PwGroupView : ClickView 
	{
		private PwGroup _pwGroup;
		private readonly GroupBaseActivity _groupBaseActivity;
		private readonly TextView _textview;

		private const int MenuOpen = Menu.First;
		private const int MenuDelete = MenuOpen + 1;
		private const int MenuMove = MenuDelete + 1;
		
		public static PwGroupView GetInstance(GroupBaseActivity act, PwGroup pw) {

			return new PwGroupView(act, pw);

		}
		public PwGroupView (IntPtr javaReference, JniHandleOwnership transfer)
			: base(javaReference, transfer)
		{
			
		}

		private PwGroupView(GroupBaseActivity act, PwGroup pw)
		: base(act){
			_groupBaseActivity = act;
			
			View gv = Inflate(act, Resource.Layout.group_list_entry, null);
			
			_textview = (TextView) gv.FindViewById(Resource.Id.group_text);
			float size = PrefsUtil.GetListTextSize(act); 
			_textview.TextSize = size;
			
			TextView label = (TextView) gv.FindViewById(Resource.Id.group_label);
			label.TextSize = size-8;

			PopulateView(gv, pw);
			
			LayoutParams lp = new LayoutParams(ViewGroup.LayoutParams.FillParent, ViewGroup.LayoutParams.WrapContent);
			
			AddView(gv, lp);
		}
		
		private void PopulateView(View gv, PwGroup pw) {
			_pwGroup = pw;
			
			ImageView iv = (ImageView) gv.FindViewById(Resource.Id.group_icon);
			App.Kp2a.GetDb().DrawableFactory.AssignDrawableTo(iv, Resources, App.Kp2a.GetDb().KpDatabase, pw.IconId, pw.CustomIconUuid);
			
			_textview.Text = pw.Name;

			//todo: get colors from resources
			if (_groupBaseActivity.IsBeingMoved(_pwGroup.Uuid))
				_textview.SetTextColor(new Color(180, 180, 180));
			else
				_textview.SetTextColor(new Color(0, 0, 0));

			
		}
		
		public void ConvertView(PwGroup pw) {
			PopulateView(this, pw);
		}
		
		public override void OnClick() {
			LaunchGroup();
		}
		
		private void LaunchGroup() {
			GroupActivity.Launch(_groupBaseActivity, _pwGroup, _groupBaseActivity.AppTask);
			_groupBaseActivity.OverridePendingTransition(Resource.Animation.anim_enter, Resource.Animation.anim_leave);

		}
		
		public override void OnCreateMenu(IContextMenu menu, IContextMenuContextMenuInfo menuInfo) {
			menu.Add(0, MenuOpen, 0, Resource.String.menu_open);
			menu.Add(0, MenuDelete, 0, Resource.String.menu_delete);
			menu.Add(0, MenuMove, 0, Resource.String.menu_move);
		}
		
		public override bool OnContextItemSelected(IMenuItem item) 
		{
			switch ( item.ItemId ) {
				
			case MenuOpen:
				LaunchGroup();
				return true;
			
			case MenuDelete:
				Handler handler = new Handler();
				DeleteGroup task = new DeleteGroup(Context, App.Kp2a, _pwGroup, new GroupBaseActivity.AfterDeleteGroup(handler, _groupBaseActivity));
				task.Start();
				return true;
			case MenuMove:
				_groupBaseActivity.StartTask(new MoveElementTask { Uuid = _pwGroup.Uuid });
				return true;
			default:
				return false;
			}
		}
		
	}
}

