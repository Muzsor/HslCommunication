﻿using HslCommunication.Core;
using HslCommunicationDemo.PLC.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace HslCommunicationDemo.DemoControl
{
	public partial class UserControlReadWriteDevice : UserControl
	{
		public UserControlReadWriteDevice( )
		{
			InitializeComponent( );

			allControls.Add( batchReadControl1 );
			allControls.Add( batchReadControl2 );
			allControls.Add( stressTesting1 );
			allControls.Add( dataExportControl1 );
		}

		private void UserControlReadWriteDevice_Load( object sender, EventArgs e )
		{
			// 加载的事件
			if (Program.Language == 2)
			{
				tabPage1.Text = "Batch Read";
				tabPage2.Text = "Frame Message Read";
				tabPage3.Text = "Thread Test";
				tabPage4.Text = "Data Table";
			}

			batchReadControl2.IsSourceReadMode = true;
		}

		private List<UserControl> allControls = new List<UserControl>( );

		public void SetEnable( bool enbale )
		{
			userControlReadWriteOp1.Enabled = enbale;
			for (int i = 0; i < allControls.Count; i++)
			{
				allControls[i].Enabled = enbale;
			}
		}

		public void SetReadWriteNet( IReadWriteNet readWrite, string address, bool isAsync = false, int strLength = 10 )
		{
			this.userControlReadWriteOp1.SetReadWriteNet( readWrite, address, isAsync, strLength );
			this.stressTesting1.SetReadWriteNet( readWrite, address );
			this.dataTableControl1.SetReadWriteNet( readWrite );
			this.dataExportControl1.SetReadWriteNet( readWrite );
		}

		public UserControlReadWriteOp ReadWriteOpControl => this.userControlReadWriteOp1;

		public BatchReadControl BatchRead
		{
			get => this.batchReadControl1;
		}

		public BatchReadControl MessageRead
		{
			get => this.batchReadControl2;
		}

		public void AddSpecialFunctionTab( UserControl control, bool show = false, string title = null )
		{
			if (control != null)
			{
				DemoUtils.AddSpecialFunctionTab( this.tabControl1, control, show, title );
				if (control is AddressExampleControl)
				{

				}
				else
				{
					allControls.Add( control );
				}
			}
		}


		private void stressTesting1_Load( object sender, EventArgs e )
		{

		}



		public void RemoveReadBatch( )
		{
			tabControl1.TabPages.Remove( tabPage1 );
		}

		public void RemoveReadMessage( )
		{
			tabControl1.TabPages.Remove( tabPage2 );
		}

		public void GetDataTable( XElement element )
		{
			this.dataTableControl1.GetDataTable( element );
		}

		public int LoadDataTable( XElement element )
		{
			return this.dataTableControl1.LoadDataTable( element );
		}

		public void SelectTabDataTable( )
		{
			this.tabControl1.SelectTab( tabPage4 );
		}
	}
}
