CREATE TABLE [dbo].[channel](
	[ChannelId] [nvarchar](255) NOT NULL,
	[CanAdd] [smallint] NOT NULL,
	[CanDelete] [smallint] NOT NULL,
	[CanView] [smallint] NOT NULL,
	[Icon] [nvarchar](255) NULL,
	[IsProtected] [smallint] NOT NULL,
	[IsPublic] [smallint] NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[ProtectionKey] [nvarchar](255) NULL,
	[UserId] [nvarchar](255) NOT NULL,
CONSTRAINT [PK_channel_ChannelId] PRIMARY KEY CLUSTERED ([ChannelId] ASC))

CREATE TABLE [dbo].[content](
	[ContentId] [nvarchar](255) NOT NULL,
	[ChannelId] [nvarchar](255) NOT NULL,
	[UserId] [nvarchar](255) NOT NULL,
	[DateCreated] [datetime2](0) NOT NULL,
	[DateModified] [datetime2](0) NOT NULL,
	[IsSummarized] [smallint] NOT NULL,
CONSTRAINT [PK_content_ContentId] PRIMARY KEY CLUSTERED ([ContentId] ASC))

CREATE TABLE [dbo].[credential](
	[UserId] [nvarchar](255) NOT NULL,
	[Password] [nvarchar](255) NOT NULL,
CONSTRAINT [PK_credential_UserId] PRIMARY KEY CLUSTERED ([UserId] ASC))

CREATE TABLE [dbo].[file](
	[ContentId] [nvarchar](255) NOT NULL,
	[Description] [nvarchar](255) NOT NULL,
	[Mime] [nvarchar](255) NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[Size] [int] NOT NULL,
CONSTRAINT [PK_file_ContentId] PRIMARY KEY CLUSTERED ([ContentId] ASC))

CREATE TABLE [dbo].[inbox](
	[MailId] [nvarchar](255) NOT NULL,
	[UserId] [nvarchar](255) NOT NULL,
	[IsFlagged] [smallint] NOT NULL,
	[IsRead] [smallint] NOT NULL,
	[InboxId] [nvarchar](255) NOT NULL,
CONSTRAINT [PK_inbox_InboxId] PRIMARY KEY CLUSTERED ([InboxId] ASC))

CREATE TABLE [dbo].[lock](
	[ChannelId] [nvarchar](255) NOT NULL,
	[ProtectionKey] [nvarchar](255) NOT NULL,
CONSTRAINT [PK_lock_ChannelId] PRIMARY KEY CLUSTERED ([ChannelId] ASC))

CREATE TABLE [dbo].[mail](
	[MailId] [nvarchar](255) NOT NULL,
	[SenderId] [nvarchar](255) NOT NULL,
	[DateSent] [datetime2](0) NOT NULL,
	[Subject] [nvarchar](255) NOT NULL,
	[Contents] [nvarchar](max) NOT NULL,
CONSTRAINT [PK_mail_MailId] PRIMARY KEY CLUSTERED ([MailId] ASC))

CREATE TABLE [dbo].[outbox](
	[MailId] [nvarchar](255) NOT NULL,
	[UserId] [nvarchar](255) NOT NULL,
	[IsFlagged] [smallint] NOT NULL,
	[OutboxId] [nvarchar](255) NOT NULL,
CONSTRAINT [PK_outbox_OutboxId] PRIMARY KEY CLUSTERED ([OutboxId] ASC))

CREATE TABLE [dbo].[post](
	[ContentId] [nvarchar](255) NOT NULL,
	[Contents] [nvarchar](max) NOT NULL,
	[ParentId] [nvarchar](255) NULL,
	[TopicId] [nvarchar](255) NULL,
CONSTRAINT [PK_post_ContentId] PRIMARY KEY CLUSTERED ([ContentId] ASC))

CREATE TABLE [dbo].[recentview](
	[RecentViewId] [varchar](255) NOT NULL,
	[DateViewed] [datetime2](0) NOT NULL,
	[ContentId] [varchar](255) NOT NULL,
	[TokenId] [varchar](255) NOT NULL,
CONSTRAINT [PK_recentview_RecentViewId] PRIMARY KEY CLUSTERED ([RecentViewId] ASC))

CREATE TABLE [dbo].[recipient](
	[MailId] [nvarchar](255) NOT NULL,
	[UserId] [nvarchar](255) NOT NULL,
CONSTRAINT [PK_recipient_MailId] PRIMARY KEY CLUSTERED ([MailId] ASC, [UserId] ASC))

CREATE TABLE [dbo].[subscription](
	[ChannelId] [nvarchar](255) NOT NULL,
	[UserId] [nvarchar](255) NOT NULL,
	[CanDelete] [smallint] NOT NULL,
	[CanAdd] [smallint] NOT NULL,
	[CanView] [smallint] NOT NULL,
	[IsActive] [smallint] NOT NULL,
	[SubscriptionId] [nvarchar](255) NOT NULL,
CONSTRAINT [PK_subscription_SubscriptionId] PRIMARY KEY CLUSTERED ([SubscriptionId] ASC))

CREATE TABLE [dbo].[token](
	[TokenId] [nvarchar](255) NOT NULL,
	[UserId] [nvarchar](255) NOT NULL,
	[DateIssued] [datetime2](0) NOT NULL,
	[DateUsed] [datetime2](0) NOT NULL,
	[IsService] [smallint] NOT NULL,
CONSTRAINT [PK_token_TokenId] PRIMARY KEY CLUSTERED ([TokenId] ASC))

CREATE TABLE [dbo].[tokenkey](
	[TokenKeyId] [nvarchar](255) NOT NULL,
	[TokenId] [nvarchar](255) NOT NULL,
	[ChannelId] [nvarchar](255) NOT NULL,
CONSTRAINT [PK_tokenkey_TokenKeyId] PRIMARY KEY CLUSTERED ([TokenKeyId] ASC))

CREATE TABLE [dbo].[topic](
	[Subject] [nvarchar](255) NOT NULL,
	[ContentId] [nvarchar](255) NOT NULL,
CONSTRAINT [PK_topic_ContentId] PRIMARY KEY CLUSTERED ([ContentId] ASC))

CREATE TABLE [dbo].[user](
	[UserId] [nvarchar](255) NOT NULL,
	[CanLogon] [smallint] NOT NULL,
	[IsListed] [smallint] NOT NULL,
	[DateAccessed] [datetime2](0) NULL,
	[DateCreated] [datetime2](0) NOT NULL,
	[DateOfLogoff] [datetime2](0) NULL,
	[DateOfLogon] [datetime2](0) NULL,
	[eMail] [nvarchar](255) NOT NULL,
	[Identity] [nvarchar](255) NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
CONSTRAINT [PK_user_UserId] PRIMARY KEY CLUSTERED ([UserId] ASC))