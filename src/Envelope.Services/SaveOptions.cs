﻿namespace Envelope.Services;

public class SaveOptions
{
	public bool? SetConcurrencyToken { get; set; }
	public bool? SetSyncToken { get; set; }
	public bool? SetCorrelationId { get; set; }
}
