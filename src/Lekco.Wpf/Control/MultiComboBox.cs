using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Lekco.Wpf.Control
{
	public class MultiComboBox : ComboBox
	{
		/// <summary>
		/// Initialize static fields.
		/// </summary>
		static MultiComboBox()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(MultiComboBox), new FrameworkPropertyMetadata(typeof(MultiComboBox)));
		}

		public IList SelectedIndexes
		{
			get => (IList)GetValue(SelectedIndexesProperty);
			set => SetValue(SelectedIndexesProperty, value);
		}
		public static readonly DependencyProperty SelectedIndexesProperty
			= DependencyProperty.Register(nameof(SelectedIndexes), typeof(IList), typeof(MultiComboBox), new PropertyMetadata(null, OnSelectedIndexesChanged));

		public IList SelectedItems
		{
			get => (IList)GetValue(SelectedItemsProperty);
			set => SetValue(SelectedItemsProperty, value);
		}
		public static readonly DependencyProperty SelectedItemsProperty
			= DependencyProperty.Register(nameof(SelectedItems), typeof(IList), typeof(MultiComboBox));

		public new string Text
		{
			get => (string)GetValue(TextProperty);
			set => SetValue(TextProperty, value);
		}
		public static readonly new DependencyProperty TextProperty
			= DependencyProperty.Register(nameof(Text), typeof(string), typeof(MultiComboBox), new PropertyMetadata(""));

		public event RoutedEventHandler SelectedItemsChanged
		{
			add => AddHandler(SelectedItemsChangedEvent, value);
			remove => RemoveHandler(SelectedItemsChangedEvent, value);
		}
		public static readonly RoutedEvent SelectedItemsChangedEvent = EventManager.RegisterRoutedEvent(nameof(SelectedItemsChanged), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MultiComboBoxItem));

		private readonly HashSet<int> _selectedIndexes;
		private readonly HashSet<MultiComboBoxItem> _selectedItems;
		private bool _resetting = false;

		public MultiComboBox()
		{
			_selectedIndexes = new HashSet<int>();
			_selectedItems = new HashSet<MultiComboBoxItem>();
			ItemContainerGenerator.StatusChanged += (s, e) =>
			{
				if (ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
				{
					Initialize();
				}
			};
			AddHandler(MultiComboBoxItem.IsSelectedChangedEvent, new RoutedEventHandler(OnIsSelectedChanged));
		}

		private void OnIsSelectedChanged(object sender, RoutedEventArgs e)
		{
			if (!_resetting && e.OriginalSource is MultiComboBoxItem mItem)
			{
				int index = Items.IndexOf(mItem);
				if (index < 0)
				{
					index = ItemContainerGenerator.IndexFromContainer(mItem);
				}
				if (mItem.IsSelected)
				{
					if (_selectedIndexes.Add(index))
					{
						SelectedIndexes?.Add(index);
					}
					if (_selectedItems.Add(mItem))
					{
						SelectedItems?.Add(mItem);
					}
				}
				else
				{
					_selectedIndexes.Remove(index);
					_selectedItems.Remove(mItem);
					SelectedIndexes?.Remove(index);
					SelectedItems?.Remove(mItem);
				}
				UpdateText();
				RaiseEvent(new RoutedEventArgs(SelectedItemsChangedEvent));
			}
		}

		private void UpdateText()
		{
			List<string> contents = new List<string>();
			foreach (var item in _selectedItems)
			{
				if (item is MultiComboBoxItem mItem)
				{
					contents.Add(mItem.Content.ToString() ?? "");
				}
			}
			Text = string.Join(", ", contents);
		}

		private static void OnSelectedIndexesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is not MultiComboBox mComboBox)
			{
				return;
			}
			mComboBox.Initialize();
		}

		protected void Initialize()
		{
			_resetting = true;
			foreach (var item in Items)
			{
				if (item is MultiComboBoxItem mItem1)
				{
					mItem1.IsSelected = false;
				}
				else if (ItemContainerGenerator.ContainerFromItem(item) is MultiComboBoxItem mItem2)
				{
					mItem2.IsSelected = false;
				}
			}
			_resetting = false;

			if (SelectedIndexes is null)
			{
				return;
			}

			foreach (int index in SelectedIndexes)
			{
				if (index >= 0 && index < Items.Count)
				{
					_selectedIndexes.Add(index);
					if (Items[index] is MultiComboBoxItem item1)
					{
						item1.IsSelected = true;
					}
					else if (ItemContainerGenerator.ContainerFromItem(Items[index]) is MultiComboBoxItem item2)
					{
						item2.IsSelected = true;
					}
				}
			}
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			if (ItemsSource != null)
			{
				foreach (object item in ItemsSource)
				{
					if (item is MultiComboBoxItem mItem)
					{
						Items.Add(mItem);
					}
				}
			}

			Initialize();
			InvalidateVisual();
		}

		protected override void AddChild(object value)
		{
			if (value is MultiComboBoxItem mItem)
			{
				Items.Add(mItem);
			}
		}

		protected override DependencyObject GetContainerForItemOverride()
		{
			return new MultiComboBoxItem();
		}

		protected override bool IsItemItsOwnContainerOverride(object item)
		{
			return item is MultiComboBoxItem;
		}
	}
}
