﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using HslCommunication.Profinet;
using HslCommunication;
using HslCommunication.Profinet.XINJE;
using System.Threading;
using System.Xml.Linq;
using HslCommunication.BasicFramework;
using HslCommunicationDemo.PLC.Common;
using HslCommunicationDemo.DemoControl;
using HslCommunicationDemo.PLC.XINJE;

namespace HslCommunicationDemo
{
	public partial class FormXinJESerialOverTcp : HslFormContent
	{
		public FormXinJESerialOverTcp( )
		{
			InitializeComponent( );
		}


		private XinJESerialOverTcp xinJE = null;
		private AddressExampleControl addressExampleControl;
		private CodeExampleControl codeExampleControl;

		private void FormSiemens_Load( object sender, EventArgs e )
		{
			comboBox_series.DataSource = SoftBasic.GetEnumValues<XinJESeries>( );

			comboBox1.SelectedIndex = 0;

			comboBox1.SelectedIndexChanged += ComboBox1_SelectedIndexChanged;
			checkBox3.CheckedChanged += CheckBox3_CheckedChanged;

			Language( Program.Language );

			addressExampleControl = new AddressExampleControl( );
			addressExampleControl.SetAddressExample( Helper.GetXinJEAddress( ) );
			userControlReadWriteDevice1.AddSpecialFunctionTab( addressExampleControl, false, DeviceAddressExample.GetTitle( ) );

			codeExampleControl = new CodeExampleControl( );
			userControlReadWriteDevice1.AddSpecialFunctionTab( codeExampleControl, false, CodeExampleControl.GetTitle( ) );
			userControlReadWriteDevice1.SetEnable( false );
		}


		private void Language( int language )
		{
			if (language == 2)
			{
				Text = "Modbus Tcp Read Demo";
				label21.Text = "station";
				checkBox1.Text = "address from 0";
				checkBox3.Text = "string reverse";
				button1.Text = "Connect";
				button2.Text = "Disconnect";
				label2.Text = "Series:";
			}
		}

		private void ComboBox1_SelectedIndexChanged( object sender, EventArgs e )
		{
			if (xinJE != null)
			{
				switch (comboBox1.SelectedIndex)
				{
					case 0: xinJE.DataFormat = HslCommunication.Core.DataFormat.ABCD;break;
					case 1: xinJE.DataFormat = HslCommunication.Core.DataFormat.BADC; break;
					case 2: xinJE.DataFormat = HslCommunication.Core.DataFormat.CDAB; break;
					case 3: xinJE.DataFormat = HslCommunication.Core.DataFormat.DCBA; break;
					default:break;
				}
			}
		}

		private void CheckBox3_CheckedChanged( object sender, EventArgs e )
		{
			if (xinJE != null)
			{
				xinJE.IsStringReverse = checkBox3.Checked;
			}
		}
		

		private void FormSiemens_FormClosing( object sender, FormClosingEventArgs e )
		{

		}
		

		#region Connect And Close



		private void button1_Click( object sender, EventArgs e )
		{
			// 连接

			if(!byte.TryParse(textBox15.Text,out byte station))
			{
				DemoUtils.ShowMessage( "Station input is wrong！" );
				return;
			}

			xinJE?.ConnectClose( );
			xinJE = new XinJESerialOverTcp( );
			xinJE.Series = (XinJESeries)comboBox_series.SelectedItem;
			xinJE.Station = station;
			xinJE.AddressStartWithZero = checkBox1.Checked;
			xinJE.LogNet = LogNet;


			ComboBox1_SelectedIndexChanged( null, new EventArgs( ) );  // 设置数据服务
			xinJE.IsStringReverse = checkBox3.Checked;

			try
			{
				this.pipeSelectControl1.IniPipe( xinJE );
				OperateResult connect = DeviceConnectPLC( xinJE );
				if (connect.IsSuccess)
				{
					DemoUtils.ShowMessage( HslCommunication.StringResources.Language.ConnectedSuccess );
					button2.Enabled = true;
					button1.Enabled = false;
					userControlReadWriteDevice1.SetEnable( true );

					// 设置基本的读写信息
					userControlReadWriteDevice1.SetReadWriteNet( xinJE, "D100", false );
					// 设置批量读取
					userControlReadWriteDevice1.BatchRead.SetReadWriteNet( xinJE, "D100", string.Empty );
					// 设置报文读取
					userControlReadWriteDevice1.MessageRead.SetReadSourceBytes( m => xinJE.ReadFromCoreServer( m, hasResponseData: true, usePackAndUnpack: false ), string.Empty, string.Empty );
					userControlReadWriteDevice1.MessageRead.SetReadSourceBytes( m => xinJE.ReadFromCoreServer( m ), "None CRC", "example: 01 03 00 00 00 01" );

					// 设置代码示例
					this.userControlReadWriteDevice1.SetDeviceVariableName( DemoUtils.PlcDeviceName );
					codeExampleControl.SetCodeText( xinJE, nameof( xinJE.Station ), nameof( xinJE.AddressStartWithZero ), nameof( xinJE.IsStringReverse ), nameof( xinJE.Series ), nameof( xinJE.DataFormat ) );
				}
				else
				{
					DemoUtils.ShowMessage( HslCommunication.StringResources.Language.ConnectedFailed + connect.Message );
				}
			}
			catch (Exception ex)
			{
				DemoUtils.ShowMessage( ex.Message );
			}
		}

		private void button2_Click( object sender, EventArgs e )
		{
			// 断开连接
			xinJE.ConnectClose( );
			button2.Enabled = false;
			button1.Enabled = true;
			userControlReadWriteDevice1.SetEnable( false );
			this.pipeSelectControl1.ExtraCloseAction( xinJE );
		}
		
		#endregion

		public override void SaveXmlParameter( XElement element )
		{
			this.pipeSelectControl1.SaveXmlParameter( element );
			element.SetAttributeValue( DemoDeviceList.XmlStation, textBox15.Text );
			element.SetAttributeValue( DemoDeviceList.XmlAddressStartWithZero, checkBox1.Checked );
			element.SetAttributeValue( DemoDeviceList.XmlDataFormat, comboBox1.SelectedIndex );
			element.SetAttributeValue( DemoDeviceList.XmlStringReverse, checkBox3.Checked );
			element.SetAttributeValue( nameof( XinJESeries ), comboBox_series.SelectedItem );

			this.userControlReadWriteDevice1.GetDataTable( element );
		}

		public override void LoadXmlParameter( XElement element )
		{
			base.LoadXmlParameter( element );
			this.pipeSelectControl1.LoadXmlParameter( element, SettingPipe.TcpPipe );
			textBox15.Text = element.Attribute( DemoDeviceList.XmlStation ).Value;
			checkBox1.Checked = bool.Parse( element.Attribute( DemoDeviceList.XmlAddressStartWithZero ).Value );
			comboBox1.SelectedIndex = int.Parse( element.Attribute( DemoDeviceList.XmlDataFormat ).Value );
			checkBox3.Checked = bool.Parse( element.Attribute( DemoDeviceList.XmlStringReverse ).Value );
			comboBox_series.SelectedItem = SoftBasic.GetEnumFromString<XinJESeries>( element.Attribute( nameof( XinJESeries ) ).Value );

			if (this.userControlReadWriteDevice1.LoadDataTable( element ) > 0)
				this.userControlReadWriteDevice1.SelectTabDataTable( );
		}

		private void userControlHead1_SaveConnectEvent_1( object sender, EventArgs e )
		{
			userControlHead1_SaveConnectEvent( sender, e );
		}
	}
}
