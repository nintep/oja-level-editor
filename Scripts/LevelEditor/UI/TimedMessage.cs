using Godot;
using System;

public partial class TimedMessage : MarginContainer
{
  [Export] private Timer _timer;
  [Export] private Label _label;

  public override void _Ready()
  {
    _label.Text = "";
    _label.Hide();
  }

  public void ShowMessage(string message, float visibleDuration =  1.0f)
  {
    _timer.WaitTime = visibleDuration;
    _label.Show();
    _label.Text = message.Trim();

    _timer.Start();
  }

  private void TimerFinished()
  {
    _label.Text = "";
    _label.Hide();
  }

  
}
