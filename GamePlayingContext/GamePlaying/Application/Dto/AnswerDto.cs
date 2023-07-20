﻿using System;

namespace GamePlaying.Application.Dto
{
    public class AnswerDto : IEquatable<AnswerDto>
    {
        public string Id { get; set; }
        public PlayerDto Player { get; set; }
        public string Name { get; set; }
        public bool IsAutoGenerated { get; set; }

        public bool Equals(AnswerDto other)
        {
            return string.Equals(other?.Name, this.Name, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }
    }
}