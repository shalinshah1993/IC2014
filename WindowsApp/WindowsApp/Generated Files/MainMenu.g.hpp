﻿

//------------------------------------------------------------------------------
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
//------------------------------------------------------------------------------
#include "pch.h"
#include "MainMenu.xaml.h"




void ::WindowsApp::MainMenu::InitializeComponent()
{
    if (_contentLoaded)
        return;

    _contentLoaded = true;

    // Call LoadComponent on ms-appx:///MainMenu.xaml
    ::Windows::UI::Xaml::Application::LoadComponent(this, ref new ::Windows::Foundation::Uri(L"ms-appx:///MainMenu.xaml"), ::Windows::UI::Xaml::Controls::Primitives::ComponentResourceLocation::Application);

    // Get the Grid named 'mainMenuGrid'
    mainMenuGrid = safe_cast<::Windows::UI::Xaml::Controls::Grid^>(static_cast<Windows::UI::Xaml::IFrameworkElement^>(this)->FindName(L"mainMenuGrid"));
    // Get the TextBlock named 'pageTitle'
    pageTitle = safe_cast<::Windows::UI::Xaml::Controls::TextBlock^>(static_cast<Windows::UI::Xaml::IFrameworkElement^>(this)->FindName(L"pageTitle"));
}

void ::WindowsApp::MainMenu::Connect(int connectionId, Platform::Object^ target)
{
    switch (connectionId)
    {
    case 1:
        (safe_cast<::Windows::UI::Xaml::Controls::Primitives::ButtonBase^>(target))->Click +=
            ref new ::Windows::UI::Xaml::RoutedEventHandler(this, (void (::WindowsApp::MainMenu::*)(Platform::Object^, Windows::UI::Xaml::RoutedEventArgs^))&MainMenu::test_Button_Click);
        break;
    case 2:
        (safe_cast<::Windows::UI::Xaml::Controls::Primitives::ButtonBase^>(target))->Click +=
            ref new ::Windows::UI::Xaml::RoutedEventHandler(this, (void (::WindowsApp::MainMenu::*)(Platform::Object^, Windows::UI::Xaml::RoutedEventArgs^))&MainMenu::doctor_Click);
        break;
    case 3:
        (safe_cast<::Windows::UI::Xaml::Controls::Primitives::ButtonBase^>(target))->Click +=
            ref new ::Windows::UI::Xaml::RoutedEventHandler(this, (void (::WindowsApp::MainMenu::*)(Platform::Object^, Windows::UI::Xaml::RoutedEventArgs^))&MainMenu::health_Click);
        break;
    case 4:
        (safe_cast<::Windows::UI::Xaml::Controls::Primitives::ButtonBase^>(target))->Click +=
            ref new ::Windows::UI::Xaml::RoutedEventHandler(this, (void (::WindowsApp::MainMenu::*)(Platform::Object^, Windows::UI::Xaml::RoutedEventArgs^))&MainMenu::analysis_Click);
        break;
    }
    (void)connectionId; // Unused parameter
    (void)target; // Unused parameter
    _contentLoaded = true;
}

