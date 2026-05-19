// Services/AppAlert.cs
// แทนที่ DisplayAlert ทุกจุดในแอป ให้อยู่ในธีมเดียวกัน
// Usage:
//   await AppAlert.ShowAsync("Title", "Message");               // Info (เขียว)
//   await AppAlert.ShowErrorAsync("Title", "Message");          // Error (แดง)
//   await AppAlert.ShowSuccessAsync("Title", "Message");        // Success (เขียว + ✓)
//   await AppAlert.ShowWarningAsync("Title", "Message");        // Warning (เหลือง)
//   bool ok = await AppAlert.ConfirmAsync("Title", "Message");  // Confirm (ยืนยัน/ยกเลิก)
using Microsoft.Maui.Controls.Shapes;

namespace POS_moblie_project.Services;

public static class AppAlert
{
    // ── Public API ────────────────────────────────────────────────

    public static Task ShowAsync(string title, string message)
        => PushAsync(new AlertConfig
        {
            Icon = "ℹ️",
            Title = title,
            Message = message,
            AccentColor = Color.FromArgb("#a9d888"),
            CardColor = Color.FromArgb("#f0f7e8"),
            BorderColor = Color.FromArgb("#c8e6a0"),
            ConfirmText = "OK"
        });

    public static Task ShowSuccessAsync(string title, string message)
        => PushAsync(new AlertConfig
        {
            Icon = "✅",
            Title = title,
            Message = message,
            AccentColor = Color.FromArgb("#a9d888"),
            CardColor = Color.FromArgb("#f0f7e8"),
            BorderColor = Color.FromArgb("#c8e6a0"),
            ConfirmText = "OK"
        });

    public static Task ShowErrorAsync(string title, string message)
        => PushAsync(new AlertConfig
        {
            Icon = "❌",
            Title = title,
            Message = message,
            AccentColor = Color.FromArgb("#E53935"),
            CardColor = Color.FromArgb("#FFF0F0"),
            BorderColor = Color.FromArgb("#FFD0D0"),
            ConfirmText = "OK"
        });

    public static Task ShowWarningAsync(string title, string message)
        => PushAsync(new AlertConfig
        {
            Icon = "⚠️",
            Title = title,
            Message = message,
            AccentColor = Color.FromArgb("#F4A620"),
            CardColor = Color.FromArgb("#FFF8E7"),
            BorderColor = Color.FromArgb("#FFE0A0"),
            ConfirmText = "OK"
        });

    public static Task<bool> ConfirmAsync(string title, string message,
        string confirmText = "Confirm", string cancelText = "Cancel",
        bool isDanger = false)
        => PushConfirmAsync(new AlertConfig
        {
            Icon = isDanger ? "🗑️" : "❓",
            Title = title,
            Message = message,
            AccentColor = isDanger ? Color.FromArgb("#E53935") : Color.FromArgb("#a9d888"),
            CardColor = isDanger ? Color.FromArgb("#FFF0F0") : Color.FromArgb("#f0f7e8"),
            BorderColor = isDanger ? Color.FromArgb("#FFD0D0") : Color.FromArgb("#c8e6a0"),
            ConfirmText = confirmText,
            CancelText = cancelText,
            HasCancel = true
        });

    // ── Internal ──────────────────────────────────────────────────

    private static async Task PushAsync(AlertConfig config)
    {
        var page = new AlertPage(config);
        await Application.Current!.MainPage!.Navigation.PushModalAsync(page, animated: false);
        await page.WaitForDismissAsync();
    }

    private static async Task<bool> PushConfirmAsync(AlertConfig config)
    {
        var page = new AlertPage(config);
        await Application.Current!.MainPage!.Navigation.PushModalAsync(page, animated: false);
        return await page.WaitForResultAsync();
    }

    // ── Action ──────────────────────────────────────────────────
    public static async Task<string?> ShowActionSheetAsync(
    string title,
    string cancel,
    params string[] options)
    {
        var page = new ActionSheetPage(title, cancel, options);
        await Application.Current!.MainPage!.Navigation.PushModalAsync(page, animated: false);
        return await page.WaitForResultAsync();
    }
}

// ── Config ────────────────────────────────────────────────────────

internal class AlertConfig
{
    public string Icon { get; set; } = "ℹ️";
    public string Title { get; set; } = "";
    public string Message { get; set; } = "";
    public Color AccentColor { get; set; } = Color.FromArgb("#a9d888");
    public Color CardColor { get; set; } = Color.FromArgb("#f0f7e8");
    public Color BorderColor { get; set; } = Color.FromArgb("#c8e6a0");
    public string ConfirmText { get; set; } = "OK";
    public string CancelText { get; set; } = "Cancel";
    public bool HasCancel { get; set; } = false;
}

// ── Alert Page ────────────────────────────────────────────────────

internal class AlertPage : ContentPage
{
    private readonly TaskCompletionSource<bool> _tcs = new();

    public Task WaitForDismissAsync() => _tcs.Task;
    public Task<bool> WaitForResultAsync() => _tcs.Task;

    public AlertPage(AlertConfig config)
    {
        BackgroundColor = Color.FromArgb("#80000000");
        Shell.SetNavBarIsVisible(this, false);

        // ── Message label ──
        var messageLabel = new Label
        {
            Text = config.Message,
            FontSize = 14,
            TextColor = Color.FromArgb("#444444"),
            HorizontalOptions = LayoutOptions.Center,
            HorizontalTextAlignment = TextAlignment.Center,
            LineBreakMode = LineBreakMode.WordWrap,
            Margin = new Thickness(0, 0, 0, 20)
        };

        // ── Buttons ──
        var buttonGrid = new Grid
        {
            ColumnSpacing = 10
        };

        if (config.HasCancel)
        {
            buttonGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            buttonGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

            var cancelBtn = new Button
            {
                Text = config.CancelText,
                BackgroundColor = Color.FromArgb("#F0F0F0"),
                TextColor = Color.FromArgb("#444444"),
                FontAttributes = FontAttributes.Bold,
                HeightRequest = 48,
                CornerRadius = 10
            };
            cancelBtn.Clicked += async (s, e) =>
            {
                await Navigation.PopModalAsync(false);
                _tcs.TrySetResult(false);
            };

            var confirmBtn = new Button
            {
                Text = config.ConfirmText,
                BackgroundColor = config.AccentColor,
                TextColor = Colors.White,
                FontAttributes = FontAttributes.Bold,
                HeightRequest = 48,
                CornerRadius = 10
            };
            confirmBtn.Clicked += async (s, e) =>
            {
                await Navigation.PopModalAsync(false);
                _tcs.TrySetResult(true);
            };

            Grid.SetColumn(cancelBtn, 0);
            Grid.SetColumn(confirmBtn, 1);
            buttonGrid.Children.Add(cancelBtn);
            buttonGrid.Children.Add(confirmBtn);
        }
        else
        {
            buttonGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

            var okBtn = new Button
            {
                Text = config.ConfirmText,
                BackgroundColor = config.AccentColor,
                TextColor = Colors.White,
                FontAttributes = FontAttributes.Bold,
                HeightRequest = 48,
                CornerRadius = 10
            };
            okBtn.Clicked += async (s, e) =>
            {
                await Navigation.PopModalAsync(false);
                _tcs.TrySetResult(true);
            };

            Grid.SetColumn(okBtn, 0);
            buttonGrid.Children.Add(okBtn);
        }

        // ── Card ──
        var card = new Border
        {
            BackgroundColor = Colors.White,
            StrokeThickness = 0,
            Padding = new Thickness(24, 24),
            WidthRequest = 320,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            StrokeShape = new RoundRectangle { CornerRadius = 20 },
            Shadow = new Shadow
            {
                Brush = new SolidColorBrush(Color.FromArgb("#00000040")),
                Offset = new Point(0, 4),
                Radius = 16,
                Opacity = 0.3f
            },
            Content = new VerticalStackLayout
            {
                Spacing = 0,
                Children =
                {
                    // Icon
                    new Label
                    {
                        Text = config.Icon,
                        FontSize = 40,
                        HorizontalOptions = LayoutOptions.Center,
                        Margin = new Thickness(0, 0, 0, 8)
                    },
                    // Title
                    new Label
                    {
                        Text = config.Title,
                        FontSize = 18,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Color.FromArgb("#222222"),
                        HorizontalOptions = LayoutOptions.Center,
                        HorizontalTextAlignment = TextAlignment.Center,
                        Margin = new Thickness(0, 0, 0, 12)
                    },
                    // Divider
                    new BoxView
                    {
                        HeightRequest = 1,
                        Color = Color.FromArgb("#F0F0F0"),
                        Margin = new Thickness(0, 0, 0, 12)
                    },
                    // Message card
                    new Border
                    {
                        BackgroundColor = config.CardColor,
                        StrokeThickness = 1,
                        Stroke = config.BorderColor,
                        Padding = new Thickness(14, 12),
                        Margin = new Thickness(0, 0, 0, 20),
                        StrokeShape = new RoundRectangle { CornerRadius = 10 },
                        Content = messageLabel
                    },
                    // Buttons
                    buttonGrid
                }
            }
        };

        // ── Dim background (tap to dismiss info only) ──
        var bg = new BoxView { BackgroundColor = Color.FromArgb("#80000000") };
        if (!config.HasCancel)
        {
            bg.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(async () =>
                {
                    await Navigation.PopModalAsync(false);
                    _tcs.TrySetResult(true);
                })
            });
        }

        Content = new Grid { Children = { bg, card } };
    }
}

// ── Action Page ────────────────────────────────────────────────────
internal class ActionSheetPage : ContentPage
{
    private readonly TaskCompletionSource<string?> _tcs = new();
    public Task<string?> WaitForResultAsync() => _tcs.Task;

    public ActionSheetPage(string title, string cancel, string[] options)
    {
        BackgroundColor = Color.FromArgb("#80000000");
        Shell.SetNavBarIsVisible(this, false);

        var optionButtons = new VerticalStackLayout { Spacing = 0 };

        foreach (var opt in options)
        {
            var btn = new Border
            {
                BackgroundColor = Colors.White,
                StrokeThickness = 0,
                Padding = new Thickness(20, 14),
            };

            btn.Content = new Label
            {
                Text = opt,
                FontSize = 15,
                TextColor = Color.FromArgb("#222222"),
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };

            btn.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(async () =>
                {
                    await Navigation.PopModalAsync(false);
                    _tcs.TrySetResult(opt);
                })
            });

            optionButtons.Children.Add(btn);
            optionButtons.Children.Add(new BoxView
            {
                HeightRequest = 1,
                Color = Color.FromArgb("#c8e6a0")
            });
        }

        // Card หลัก
        var card = new Border
        {
            BackgroundColor = Colors.White,
            StrokeThickness = 0,
            Margin = new Thickness(20, 0),
            VerticalOptions = LayoutOptions.End,
            StrokeShape = new RoundRectangle { CornerRadius = 20 },
            Shadow = new Shadow
            {
                Brush = new SolidColorBrush(Color.FromArgb("#00000040")),
                Offset = new Point(0, 4),
                Radius = 16,
                Opacity = 0.3f
            },
            Content = new VerticalStackLayout
            {
                Spacing = 0,
                Children =
                {
                    // Icon + Title
                    new VerticalStackLayout
                    {
                        Spacing = 4,
                        Padding = new Thickness(20, 20, 20, 16),
                        Children =
                        {
                            new Label
                            {
                                Text = "🤖",
                                FontSize = 32,
                                HorizontalOptions = LayoutOptions.Center
                            },
                            new Label
                            {
                                Text = title,
                                FontSize = 16,
                                FontAttributes = FontAttributes.Bold,
                                TextColor = Color.FromArgb("#2d6a2d"),
                                HorizontalOptions = LayoutOptions.Center,
                                HorizontalTextAlignment = TextAlignment.Center
                            }
                        }
                    },
                    // Divider
                    new BoxView { HeightRequest = 1, Color = Color.FromArgb("#c8e6a0") },
                    // Options
                    optionButtons
                }
            }
        };

        // Cancel button — แยกออกมา
        var cancelBorder = new Border
        {
            BackgroundColor = Colors.White,
            StrokeThickness = 0,
            Padding = new Thickness(20, 14),
            Margin = new Thickness(20, 10, 20, 0),
            StrokeShape = new RoundRectangle { CornerRadius = 14 },
            Shadow = new Shadow
            {
                Brush = new SolidColorBrush(Color.FromArgb("#00000020")),
                Offset = new Point(0, 2),
                Radius = 8,
                Opacity = 0.2f
            },
            Content = new Label
            {
                Text = cancel,
                FontSize = 15,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#E53935"),
                HorizontalOptions = LayoutOptions.Center
            }
        };
        cancelBorder.GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = new Command(async () =>
            {
                await Navigation.PopModalAsync(false);
                _tcs.TrySetResult(null);
            })
        });

        // Dim background
        var bg = new BoxView { BackgroundColor = Color.FromArgb("#80000000") };
        bg.GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = new Command(async () =>
            {
                await Navigation.PopModalAsync(false);
                _tcs.TrySetResult(null);
            })
        });

        Content = new Grid
        {
            Children =
            {
                bg,
                new VerticalStackLayout
                {
                    VerticalOptions = LayoutOptions.End,
                    Padding = new Thickness(0, 0, 0, 40),
                    Children = { card, cancelBorder }
                }
            }
        };
    }
}